# Unified manifests boot — design

**ADO:** #63380 ("Fix import leaks, bundle manifests/contexts, add lazy loading")
**Branch:** `v17/improvement/63380-unified-manifests-boot`
**Date:** 2026-05-20
**Status:** Infrastructure landed; runtime switch deferred (see "Known issue: custom-element duplication" at the bottom).

## Goal

Collapse the backoffice boot-time manifest layer (currently ~187 chunks across two
serial waves — 79 entry chunks plus their static-import closure) into a single
Rollup-merged artifact loaded once at boot. Reduce request count without changing
the public extension contract or the runtime registration semantics.

## Today's boot flow

`app.element.ts` constructs a `CORE_PACKAGES` array of 37 dynamic `import()`
calls — one per first-party package. Each `umbraco-package.ts` exports a bundle
manifest:

```ts
export const extensions = [{
  type: 'bundle',
  alias: 'Umb.Bundle.Foo',
  js: () => import('./manifests.js'),
}];
```

`UmbBundleExtensionInitializer` then fires every bundle's `js()` to load the
actual manifests. This is two serial waves:

1. 37 `umbraco-package.js` chunks (the dynamic-import array fires at module eval)
2. 37 `manifests.js` chunks (bundle initializer fires after step 1 modules execute)

Measured production eager set (rooted at every package's `umbraco-package.js`
and `manifests.js` files in `dist-cms`, walking static imports transitively):

| Metric | Today |
|---|---:|
| Entry chunks loaded | 79 |
| Eager chunks (incl. closure) | 187 |
| Raw bytes | 1 032 KB |
| Gzipped bytes | 267 KB |
| Boot waterfall depth | 2 serial waves |

## Proposed design

### Step 2: replace `CORE_PACKAGES` with a single merged manifests import

A root-level Vite build emits one `manifests-all.js` containing every first-party
package's manifests, statically reachable from one entry. The artifact is added
to the importmap so it can be imported via a stable specifier (e.g.
`@umbraco-cms/backoffice/manifests-all`). `app.element.ts` drops the
`CORE_PACKAGES` array and `#registerExtensions` becomes:

```ts
async #registerExtensions() {
  const { allManifests } = await import('@umbraco-cms/backoffice/manifests-all');
  umbExtensionsRegistry.registerMany(allManifests);
}
```

The per-package `umbraco-package.ts` files remain for the third-party plugin
contract (each plugin still exports `{ name, extensions }`) but the first-party
ones are no longer loaded individually at boot — their content lives inside
`manifests-all.js`. The bundle-wrapper indirection and the second boot wave
(bundle-initializer firing `js()` per package) disappear for first-party
packages.

### Vite plugin shape

A new build-time plugin walks `src/packages/*/manifests.ts`, generates a single
virtual entry file that statically imports them all, and emits the result as
`manifests-all.js`. Schematically:

```ts
import { unifiedManifestsPlugin } from './devops/build/unified-manifests-plugin';

export default defineConfig({
  // ... existing root vite config
  plugins: [
    unifiedManifestsPlugin({
      include: 'src/packages/*/manifests.ts',
      output: 'manifests-all',
    }),
  ],
});
```

Plugin responsibilities:
- Resolve the glob at build start; emit a virtual module that imports
  `{ manifests as <pkg> }` from each match and exports `allManifests`.
- Register the virtual module as a Rollup entry so chunking sees the full graph.
- In dev mode: no-op for the entry generation (Vite serves source modules
  individually anyway). Optionally accept HMR by invalidating the virtual
  module when any matched file changes.

### Measured Step 2 result

From the trial build in `devops/manifests-trial/`:

| Metric | Today | Step 2 | Δ |
|---|---:|---:|---:|
| Entry chunks loaded | 79 | 1 | −78 |
| Eager chunks (incl. closure) | 187 | 36 | **−151 (−81 %)** |
| Raw bytes | 1 032 KB | 964 KB | −68 KB |
| Gzipped bytes | 267 KB | 208 KB | **−59 KB (−22 %)** |
| Boot waterfall depth | 2 serial waves (umbraco-package, then bundle js) | 1 wave (one entry + its 35 static-import chunks fetched in parallel) | −1 wave |
| Build wall-clock (merged graph) | n/a | 91 s | +63 s vs per-workspace |

The byte reduction comes from single-graph Rollup deduplication across packages
that per-workspace builds can't see (most notably a 198 KB shared
`script-item.store.context-token` chunk that today is duplicated across many
manifest closures).

## Dev mode

Unchanged from today's experience. Vite dev server serves source modules
on-demand and HMRs individual files. The plugin's virtual entry only affects
production builds. Confirmed by trial.

## Migration prerequisites

Both completed in the preceding commit (58cd57e36e9):

- `documents/umbraco-package.ts` normalised to lazy-bundle pattern.
- `umbraco-news/manifests.ts` normalised to export `manifests: Array<...>`.

All 39 first-party packages now use a uniform shape, which makes the plugin's
input contract trivial.

## What does NOT change

- Third-party plugin contract. Plugins continue to ship `umbraco-package.ts`
  with `{ name, extensions }`; the server registers them, and the bundle
  initializer fires for their lazy `js()` exactly as today.
- The extension registry API surface.
- The bundle extension type itself — it remains supported for third-party use
  and for any first-party manifests that genuinely benefit from lazy loading
  in a future iteration.
- Importmap topology — `@umbraco-cms/*` cross-workspace boundaries remain
  externalised in the per-workspace builds. The merged manifests artifact is
  added alongside, not in place of, the workspace outputs.

## Measurement gate (before merge to v17/dev)

Run `devops/measure-ttfe.mjs` with `NETWORK=wan` and `NETWORK=fast-3g` on a
content document with TipTap + Block Grid (the GH #21152 worst case). Expect:

- Boot JS request count: −34 to −74 (depending on which waves get coalesced).
- WAN TTFE: improvement on top of PR #22896's 5 321 → 4 394 ms.
- Localhost TTFE: flat or slightly improved (one fewer waterfall wave should
  show even when RTT≈0).
- 3G TTFE: bytes-bound, expect modest improvement from the gzip reduction.

If WAN TTFE does not improve by at least ~5 %, fall back to Step 1 only:
merge `umbraco-package.ts` files (keeping the bundle indirection) and ship
that as the smaller intermediate win. Step 1 was modelled but not measured —
re-measure before shipping that variant.

## Rollback

The plugin is additive: removing it from the root vite config and reverting
the `app.element.ts` change restores the previous boot flow without touching
the per-workspace builds or the bundle initializer.

## Out of scope

- Reducing element/repository/context chunk counts (covered by ADO #68483).
- Locale chunk pruning (#68484), Monaco lazy loading (#68487), and the
  Lit-hoist core consolidation (#68485 → #63381) — all independent and
  separately tracked.
- Codemod converting by-value class refs in manifests to lazy imports
  (#68481). The trial showed this is **no longer a prerequisite** for unifying
  manifests: by-value classes are already in the eager set today, so merging
  is byte-neutral. The codemod can still be done later as a separate
  optimization to shrink the eager set further, but it does not gate this work.

## Open questions

- Naming of the public import path. `@umbraco-cms/backoffice/manifests-all`
  is descriptive but verbose; alternatives welcome.
- Whether to keep first-party `umbraco-package.ts` files as compatibility
  stubs or remove them entirely (they would become dead code after the
  `app.element.ts` change). Recommend keeping them for one release as
  reachable-but-unused files, then removing in the following major.

## Known issue: custom-element duplication (runtime switch deferred)

When the runtime switch was attempted (single import of
`@umbraco-cms/backoffice/manifests-all` replacing `CORE_PACKAGES`), the
backoffice failed to boot with `NotSupportedError: Failed to execute 'define'
on 'CustomElementRegistry': the name "umb-block-action-list" has already
been used with this registry`.

**Root cause.** The merged manifests-all Rollup graph statically reaches the
implementation source of many custom elements (via the manifest sub-tree of
each package — e.g. `block/manifests.ts` → sub-feature manifests → store /
context-token modules → element side-effect imports). Rollup bundles those
element sources into chunks under `dist-cms/manifests-all/`. The per-workspace
builds **also** emit the same elements into `dist-cms/packages/<pkg>/`. At
runtime, when something in the merged graph hits an externalised
`@umbraco-cms/backoffice/block` import, the importmap resolves it to the
per-workspace chunk, which calls `customElements.define(...)`. Then the
merged graph's own copy fires the same define and crashes.

**Why it's hard.** Fixing this requires either:

1. Externalising relative imports beyond the manifest boundary in the merged
   build (so element implementation source is not bundled twice). The
   "boundary" is fuzzy — manifest literals reach by-value class refs which
   reach element classes — and expressing it in Rollup config is brittle.
2. Skipping per-workspace builds for content that lands in manifests-all —
   breaks the cross-workspace plugin contract.
3. Reverting to bundle-indirection (Step 1 of the original brainstorm): the
   merged file holds bundle wrappers only, and `manifests.ts` continues to
   load per-workspace via the bundle initializer's `js()` callback. Smaller
   win (saves one wave, not both) but no duplication.

**Decision for this PR.** Land Tasks 1–3 (plugin, build config, importmap
exposure, plus the type stub) as additive infrastructure. The artifact is
built and reachable at `@umbraco-cms/backoffice/manifests-all` but nothing
imports it at runtime. The runtime switch (Task 4) is reverted and tracked
as follow-up work that has to resolve the duplication first.

The normalisation of three packages whose `umbraco-package.ts` had inlined
`backofficeEntryPoint` extensions (media, property-editors, templating)
**is kept** — those declarations now live in their respective `manifests.ts`
files, which is a cleaner location regardless of whether the runtime switch
ever lands. The bundle initializer continues to load them per-workspace.
