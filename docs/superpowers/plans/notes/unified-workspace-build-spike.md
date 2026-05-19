# Spike: unified workspace Vite build

**Date:** 2026-05-19
**Context:** Follow-up to AB#67983 (core-bundling) and GH #21152 (backoffice perf). Goal: cut build wall-clock by replacing `npm run build --workspaces --if-present` (58 sequential Vite invocations) with a single Vite/Rollup invocation that builds every workspace as part of one chunk graph.

## Topology

- **Workspaces with their own `vite.config.ts`:** 58 (`src/libs/* + src/packages/* + src/external/*`).
- **Uniformity:** every config uses the shared `getDefaultConfig` from `src/vite-config-base.ts`. None set a custom `external`. None set custom Rollup options. None set `inlineDynamicImports` / `preserveModules` / `preserveEntrySignatures`. Only 19 of 58 set a `base:`.
- **Output layout:** every workspace writes to `dist-cms/<libs|packages|external>/<name>/...`. All use ES modules, sourcemaps, `target: es2022`.
- **Externalization:** the default `external: [/^@umbraco-cms/]` means every cross-workspace reference resolves to a sibling workspace's published path at runtime. This is what makes the per-workspace build correct today — `documents/` doesn't accidentally inline `core/` code, because `@umbraco-cms/backoffice/auth` (etc.) is treated as a bare external.
- **Side-effects in build:** only 2 of 58 do anything beyond `defineConfig({ ...getDefaultConfig(...) })`:
  - `external/uui` copies `uui-css.css` + fonts into `dist-cms/css` and `dist-cms/assets/fonts`.
  - `external/monaco-editor` does similar asset copying.

## Viability

**Can we collapse into a single config?** Yes, with caveats.

**Sketch of the unified shape**

```ts
// vite.config.ts at the client root
import { defineConfig } from 'vite';
import { workspaces } from './devops/build/discover-workspaces.js';

const allEntries = Object.fromEntries(
  workspaces.flatMap((ws) =>
    Object.entries(ws.entries).map(([k, v]) => [
      // namespace the entry key so output paths don't collide
      `${ws.relativeDist}/${k}`,
      `${ws.dir}/${v}`,
    ]),
  ),
);

export default defineConfig({
  build: {
    outDir: 'dist-cms',
    emptyOutDir: true,
    sourcemap: true,
    lib: { entry: allEntries, formats: ['es'] },
    rollupOptions: {
      external: [/^@umbraco-cms/], // keep boundary semantics
      output: {
        entryFileNames: '[name].js',
        chunkFileNames: 'shared/[name]-[hash].js',
        experimentalMinChunkSize: 10_000,
      },
    },
  },
});
```

The `workspaces` discovery would parse each workspace's `package.json` + `vite.config.ts` (or, more cleanly, refactor each `vite.config.ts` into an exported manifest the root collects).

## Blockers

1. **Cross-workspace shared chunks.** Today every workspace externalizes `@umbraco-cms/*`. In a unified build, if we keep `external: [/^@umbraco-cms/]`, every cross-workspace reference still resolves as bare-spec → not bundled together → we don't actually share chunks across workspaces, only across entries within the same workspace. **Net effect: build time saved (single Vite startup), but the chunk graph is unchanged.** If we drop the externalization, Rollup will resolve `@umbraco-cms/backoffice/auth` to its source, walk the graph across workspaces, and merge shared deps. That's a *bigger* win but also a *bigger* risk — it changes the runtime chunk topology and the importmap that `dist-cms/importmap.json` was generated against.
2. **FS side-effects in uui / monaco-editor.** Today they run inside `defineConfig` evaluation, which is per-workspace. A unified config needs these moved to a Vite plugin that runs once (`buildStart` hook), or to a pre-build step in `devops/build/`.
3. **`generate:manifest` / `package:validate` / `copy-to-cms.js` chains.** The `build:for:cms` script depends on the per-workspace dist layout. If output paths change, those tools break. Need to keep `dist-cms/<libs|packages|external>/<name>/...` byte-identical.
4. **Path collisions.** Two workspaces could have an entry called `index`; today they're isolated by `outDir`, but a unified `entry` map needs unique keys. Solvable by namespacing keys with the workspace path, but the output mapping needs to keep producing the same on-disk paths.
5. **Per-workspace `publint` / `npm publish` story.** The `prepack` script under `cleanse-pkg.js` walks each workspace's `package.json` and treats them as separate publish units. A unified build doesn't change *publishing*, but it does change how `dist-cms` is repopulated between local dev sessions — `emptyOutDir: true` on the root config would wipe everything every build, where today each workspace only wipes its own subtree.

## Estimated effort

**Medium** (3–7 working days). The straightforward unified config is small. The tricky parts are the FS side-effects, the path-mapping function for `entryFileNames`, and validating that all 58 workspace outputs land in exactly the same on-disk locations as today. Some refactoring of `devops/build/copy-to-cms.js` and possibly `cleanse-pkg.js` is likely.

## Estimated build-time win

**Speculative — needs a prototype to confirm.** Current `npm run build --workspaces` wall-clock on jov's machine is ~1–2 minutes. The dominant cost is Vite startup + tsc-equivalent transformation per workspace; both are largely sequential today. A unified build amortizes Vite startup once and lets Rollup parallelize transforms via its worker pool.

Best-case estimate: **30–50% wall-clock reduction**, conditional on keeping `external: [/^@umbraco-cms/]` so the chunk graph stays the same.

Stretch case: dropping cross-workspace externalization could *increase* chunk-sharing wins (more dedup, fewer requests at runtime) at the cost of a much larger semantic change. Worth a follow-up spike but **not** what this investigation targets.

## Recommendation

**Prototype as a separate PR.** This is a build-time optimization, distinct from the runtime perf work in #21152. Keeping it separate lets us:
- Ship the runtime wins (AB#67983 minChunkSize) without taking on the topology risk.
- Iterate on the unified build against `v17/dev` where breakage is contained.
- Measure the build-time win in isolation, without confusing it with bundling wins.

The first prototype should target *only* the same chunk graph as today (keep `external: [/^@umbraco-cms/]`) so the win is purely Vite-startup amortization. Cross-workspace dedup is a follow-up after that proves out.

## Next steps if pursued

1. Write `devops/build/discover-workspaces.js` that parses each workspace `package.json` + `vite.config.ts` and emits the unified entry map.
2. Add a Vite plugin (`umb-workspace-side-effects`) that owns the uui + monaco-editor asset copies in `buildStart`.
3. Add a root `vite.config.ts` driving the unified build.
4. Verify byte-identical output for at least 10 sample chunks before/after. Diff the importmap. Diff `dist-cms` listing.
5. Bench build wall-clock cold-cache and warm-cache against the existing per-workspace build.
6. Open as a draft PR with metrics in the description.
