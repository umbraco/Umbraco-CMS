# Unified Manifests Boot — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Collapse the backoffice boot-time manifest layer from two serial waves of ~187 chunks into one merged artifact loaded once at boot. Replace `app.element.ts`'s 37-entry `CORE_PACKAGES` array with a single import of an `allManifests` array, emitted by a build-time Vite plugin.

**Architecture:** A new build-time Vite plugin walks `src/packages/*/manifests.ts`, emits a virtual entry module that statically imports every first-party `manifests` export, and bundles them into a single Rollup-chunked artifact at `dist-cms/manifests-all/index.js`. The artifact is exposed via `package.json` exports as `@umbraco-cms/backoffice/manifests-all`, picked up automatically by the existing importmap pipeline. `app.element.ts` drops `CORE_PACKAGES` in favour of one dynamic import + `registerMany(allManifests)`.

**Tech Stack:** TypeScript, Vite 7, Rollup, Node `--test` (for pure-function unit tests), existing per-workspace build orchestration.

**Spec:** [docs/superpowers/specs/2026-05-20-unified-manifests-design.md](../specs/2026-05-20-unified-manifests-design.md)
**ADO:** #63380
**Branch:** `v17/improvement/63380-unified-manifests-boot` (already checked out)

---

## File Map

**Create:**
- `src/Umbraco.Web.UI.Client/devops/build/vite-plugin-unified-manifests.ts` — the Vite plugin (virtual module resolver + entry-source generator).
- `src/Umbraco.Web.UI.Client/devops/build/vite-plugin-unified-manifests.test.mjs` — Node `--test` unit tests for the pure entry-source generator function.
- `src/Umbraco.Web.UI.Client/devops/build/manifests-all.vite.config.ts` — dedicated Vite config consuming the plugin.

**Modify:**
- `src/Umbraco.Web.UI.Client/package.json` — add `build:manifests-all` script; wire into `build:for:cms`; add `./manifests-all` to `exports`.
- `src/Umbraco.Web.UI.Client/src/apps/app/app.element.ts` — drop `CORE_PACKAGES`, replace `#registerExtensions`.

**Reference (read, don't modify):**
- `src/Umbraco.Web.UI.Client/src/vite-config-base.ts` — base config helper.
- `src/Umbraco.Web.UI.Client/devops/importmap/index.js` — importmap auto-includes `exports` entries.
- `src/Umbraco.Web.UI.Client/devops/build/create-umbraco-package.js` — generates the dist-cms importmap.
- `src/Umbraco.Web.UI.Client/devops/measure-ttfe.mjs` — measurement script for the final verification.

---

## Task 1: Pure entry-source generator (TDD)

The plugin's only non-trivial logic is generating the virtual entry's TypeScript source from a list of package directories. That's a pure function — easy to test in isolation.

**Files:**
- Create: `src/Umbraco.Web.UI.Client/devops/build/vite-plugin-unified-manifests.ts`
- Create: `src/Umbraco.Web.UI.Client/devops/build/vite-plugin-unified-manifests.test.mjs`

- [ ] **Step 1: Write the failing test**

Create `src/Umbraco.Web.UI.Client/devops/build/vite-plugin-unified-manifests.test.mjs`:

```js
import { test } from 'node:test';
import assert from 'node:assert/strict';
import { buildEntrySource } from './vite-plugin-unified-manifests.ts';

test('emits import + spread for each package', () => {
	const src = buildEntrySource([
		{ name: 'block', path: '../../src/packages/block/manifests.ts' },
		{ name: 'documents', path: '../../src/packages/documents/manifests.ts' },
	]);
	assert.match(src, /import \{ manifests as block \} from "\.\.\/\.\.\/src\/packages\/block\/manifests\.ts"/);
	assert.match(src, /import \{ manifests as documents \} from "\.\.\/\.\.\/src\/packages\/documents\/manifests\.ts"/);
	assert.match(src, /export const allManifests = \[\s*\.\.\.block,\s*\.\.\.documents,\s*\]/);
});

test('sanitises package names with hyphens into valid identifiers', () => {
	const src = buildEntrySource([{ name: 'code-editor', path: '/abs/code-editor/manifests.ts' }]);
	assert.match(src, /import \{ manifests as codeEditor \} from /);
	assert.match(src, /\.\.\.codeEditor/);
});

test('returns empty allManifests for empty input', () => {
	const src = buildEntrySource([]);
	assert.match(src, /export const allManifests = \[\s*\]/);
});

test('produces deterministic output (stable order)', () => {
	const a = buildEntrySource([
		{ name: 'a', path: '/a' },
		{ name: 'b', path: '/b' },
	]);
	const b = buildEntrySource([
		{ name: 'a', path: '/a' },
		{ name: 'b', path: '/b' },
	]);
	assert.equal(a, b);
});
```

- [ ] **Step 2: Run test to verify it fails**

Run from `src/Umbraco.Web.UI.Client`:

```bash
node --test --experimental-strip-types devops/build/vite-plugin-unified-manifests.test.mjs
```

Expected: FAIL — `ERR_MODULE_NOT_FOUND` (`vite-plugin-unified-manifests.ts` doesn't exist yet).

- [ ] **Step 3: Implement the minimal plugin file with the pure function**

Create `src/Umbraco.Web.UI.Client/devops/build/vite-plugin-unified-manifests.ts`:

```ts
import { readdirSync, existsSync, statSync } from 'node:fs';
import path from 'node:path';
import type { Plugin } from 'vite';

export interface PackageEntry {
	/** Identifier-safe name (e.g. `codeEditor`). */
	name: string;
	/** Absolute or virtual-resolvable path to the package's manifests.ts. */
	path: string;
}

/** Convert a kebab-case package directory name into a JS identifier. */
export function toIdentifier(pkgDir: string): string {
	return pkgDir.replace(/-([a-z])/g, (_, c) => c.toUpperCase());
}

/**
 * Generate the virtual entry's TypeScript source. Pure function: takes the
 * resolved package list and emits one import per package plus a spread-array
 * named `allManifests`.
 */
export function buildEntrySource(packages: PackageEntry[]): string {
	const imports = packages
		.map((p) => `import { manifests as ${p.name} } from "${p.path}";`)
		.join('\n');
	const spreads = packages.map((p) => `\t...${p.name},`).join('\n');
	return `${imports}\n\nexport const allManifests = [\n${spreads}\n];\n`;
}

/**
 * Walk a `packages` root directory and return one PackageEntry per child
 * directory that contains a top-level `manifests.ts`.
 */
export function discoverPackages(packagesRoot: string): PackageEntry[] {
	if (!existsSync(packagesRoot)) return [];
	const entries: PackageEntry[] = [];
	for (const dir of readdirSync(packagesRoot).sort()) {
		const abs = path.join(packagesRoot, dir);
		if (!statSync(abs).isDirectory()) continue;
		const manifestPath = path.join(abs, 'manifests.ts');
		if (!existsSync(manifestPath)) continue;
		entries.push({ name: toIdentifier(dir), path: manifestPath });
	}
	return entries;
}

const VIRTUAL_ID = 'virtual:unified-manifests';
const RESOLVED_VIRTUAL_ID = '\0' + VIRTUAL_ID;

export interface UnifiedManifestsPluginOptions {
	/** Absolute path to the directory containing per-package folders. */
	packagesRoot: string;
}

/**
 * Vite plugin that exposes a virtual module aggregating every package's
 * `manifests` export into a single `allManifests` array.
 */
export function unifiedManifestsPlugin(opts: UnifiedManifestsPluginOptions): Plugin {
	let cachedSource: string | null = null;
	const refresh = () => {
		cachedSource = buildEntrySource(discoverPackages(opts.packagesRoot));
	};

	return {
		name: 'umbraco:unified-manifests',
		buildStart() {
			refresh();
		},
		resolveId(id) {
			if (id === VIRTUAL_ID) return RESOLVED_VIRTUAL_ID;
			return null;
		},
		load(id) {
			if (id !== RESOLVED_VIRTUAL_ID) return null;
			if (cachedSource === null) refresh();
			return cachedSource;
		},
		handleHotUpdate(ctx) {
			if (!ctx.file.endsWith('/manifests.ts')) return;
			refresh();
			const mod = ctx.server.moduleGraph.getModuleById(RESOLVED_VIRTUAL_ID);
			if (mod) return [mod];
		},
	};
}
```

- [ ] **Step 4: Run test to verify all four pass**

Run from `src/Umbraco.Web.UI.Client`:

```bash
node --test --experimental-strip-types devops/build/vite-plugin-unified-manifests.test.mjs
```

Expected: 4 tests pass.

- [ ] **Step 5: Commit**

```bash
git add devops/build/vite-plugin-unified-manifests.ts devops/build/vite-plugin-unified-manifests.test.mjs
git commit -m "Backoffice: Add unified manifests Vite plugin (AB#63380)

Introduces a build-time plugin that walks src/packages/*/manifests.ts
and exposes a virtual:unified-manifests module aggregating every
package's manifests export into one allManifests array. Includes
HMR invalidation so dev mode picks up manifest edits.

Pure entry-source generator covered by node --test."
```

---

## Task 2: Dedicated Vite config + npm script

Wire the plugin into a Vite build that emits `dist-cms/manifests-all/index.js`.

**Files:**
- Create: `src/Umbraco.Web.UI.Client/devops/build/manifests-all.vite.config.ts`
- Modify: `src/Umbraco.Web.UI.Client/package.json`

- [ ] **Step 1: Write the Vite config**

Create `src/Umbraco.Web.UI.Client/devops/build/manifests-all.vite.config.ts`:

```ts
import { defineConfig } from 'vite';
import path from 'node:path';
import { getDefaultConfig } from '../../src/vite-config-base';
import { unifiedManifestsPlugin } from './vite-plugin-unified-manifests';

const root = path.resolve(__dirname, '../..');
const dist = path.resolve(root, 'dist-cms/manifests-all');

export default defineConfig({
	...getDefaultConfig({
		dist,
		entry: { index: 'virtual:unified-manifests' },
	}),
	plugins: [
		unifiedManifestsPlugin({
			packagesRoot: path.resolve(root, 'src/packages'),
		}),
	],
});
```

- [ ] **Step 2: Add the npm script**

Open `src/Umbraco.Web.UI.Client/package.json` and add the `build:manifests-all` script alongside the other build scripts. Locate the existing line:

```json
"build:workspaces": "npm run build -ws --if-present",
```

Add immediately after it:

```json
"build:manifests-all": "vite build -c devops/build/manifests-all.vite.config.ts",
```

Then modify `build:for:cms` (insert `build:manifests-all` between `build:workspaces` and `generate:manifest`):

```json
"build:for:cms": "npm run build && npm run build:workspaces && npm run build:manifests-all && npm run generate:manifest && npm run package:validate && node ./devops/build/copy-to-cms.js",
```

- [ ] **Step 3: Verify the build runs end-to-end**

Run from `src/Umbraco.Web.UI.Client`:

```bash
npm run build:workspaces && npm run build:manifests-all
```

Expected:
- `build:workspaces` finishes without error.
- `build:manifests-all` finishes within ~2 minutes (single-graph Rollup is slow).
- Output appears at `dist-cms/manifests-all/index.js`.

- [ ] **Step 4: Inspect the output sanity check**

Run from `src/Umbraco.Web.UI.Client`:

```bash
ls -la dist-cms/manifests-all/ && head -3 dist-cms/manifests-all/index.js
```

Expected: `index.js` exists; head shows a small entry that exports `allManifests` and `import`s shared chunks from the same directory.

- [ ] **Step 5: Commit**

```bash
git add devops/build/manifests-all.vite.config.ts package.json
git commit -m "Backoffice: Wire unified manifests build into pipeline (AB#63380)

Dedicated Vite config consumes the unified-manifests plugin and emits
dist-cms/manifests-all/index.js. Slotted between build:workspaces and
generate:manifest in build:for:cms so the importmap step picks up the
new artifact."
```

---

## Task 3: Expose the artifact via importmap

`devops/importmap/index.js` derives the importmap from `package.json`'s `exports` field. Adding `./manifests-all` makes the artifact loadable as `@umbraco-cms/backoffice/manifests-all`.

**Files:**
- Modify: `src/Umbraco.Web.UI.Client/package.json` (exports field)

- [ ] **Step 1: Add the export entry**

Open `src/Umbraco.Web.UI.Client/package.json` and locate the `"exports"` field. Find the alphabetical neighbour `"./manifest"` if it exists, or the closest alphabetical position (`./manifests-all` sits between `./manifest` and `./markdown-editor`).

Add the entry:

```json
"./manifests-all": "./dist-cms/manifests-all/index.js",
```

- [ ] **Step 2: Regenerate the importmap and verify the entry appears**

Run from `src/Umbraco.Web.UI.Client`:

```bash
npm run generate:manifest
grep -o '"@umbraco-cms/backoffice/manifests-all":"[^"]*"' dist-cms/umbraco-package.json
```

Expected: prints `"@umbraco-cms/backoffice/manifests-all":"/umbraco/backoffice/manifests-all/index.js"`.

- [ ] **Step 3: Commit**

```bash
git add package.json
git commit -m "Backoffice: Expose manifests-all in package exports (AB#63380)

Adds the new artifact to package.json exports so the importmap pipeline
auto-includes it at @umbraco-cms/backoffice/manifests-all."
```

---

## Task 4: Switch app.element.ts to the unified import

Drop the 37-entry `CORE_PACKAGES` array and replace `#registerExtensions` with one dynamic import.

**Files:**
- Modify: `src/Umbraco.Web.UI.Client/src/apps/app/app.element.ts`

- [ ] **Step 1: Remove the CORE_PACKAGES array**

Open `src/Umbraco.Web.UI.Client/src/apps/app/app.element.ts`.

Delete lines 35–73 (the entire `const CORE_PACKAGES: Array<...> = [ ... ];` block including its 37 dynamic imports).

- [ ] **Step 2: Simplify the type imports**

The `ManifestBase` and `UmbExtensionManifestKind` type imports were used for `CORE_PACKAGES`'s declared type. The new flow keeps `UmbExtensionManifestKind` (still referenced in `#packageModules` if you keep that field). Replace the field declaration on lines ~187–188 with:

```ts
#allManifestsLoaded?: Promise<void>;
```

Delete the unused `#packageModules` field, and remove `ManifestBase` from the `@umbraco-cms/backoffice/extension-api` import if it becomes unused (run `npm run lint:errors` after Step 4 to catch this).

- [ ] **Step 3: Replace #registerExtensions**

Locate the existing `async #registerExtensions()` method (around line 322). Replace its body so the method reads:

```ts
async #registerExtensions() {
	if (this.#allManifestsLoaded === undefined) {
		this.#allManifestsLoaded = import('@umbraco-cms/backoffice/manifests-all').then((mod) => {
			umbExtensionsRegistry.registerMany(mod.allManifests);
			this.#loadCurrentUser();
		});
	}
	await this.#allManifestsLoaded;
}
```

- [ ] **Step 4: Update #loadCurrentUser's guard**

`#loadCurrentUser` (around line 344) guards on `this.#packageModules`. Change the guard to use the new field:

```ts
#loadCurrentUser() {
	if (!this.#currentUser || !this.#allManifestsLoaded) return;
	this.#currentUser.load();
}
```

- [ ] **Step 5: Lint and type-check**

Run from `src/Umbraco.Web.UI.Client`:

```bash
npm run lint:errors && npm run compile
```

Expected: both succeed. If `ManifestBase` import is now unused, the linter reports it — remove it from the import.

- [ ] **Step 6: Commit**

```bash
git add src/apps/app/app.element.ts
git commit -m "Backoffice: Load all first-party manifests via single import (AB#63380)

Drops the 37-entry CORE_PACKAGES dynamic-import array in favour of one
import of @umbraco-cms/backoffice/manifests-all. The merged artifact
contains every first-party package's manifests, eliminating the
bundle-initializer second wave for first-party packages.

Third-party umbraco-package.ts entries are unaffected — the bundle
extension type is still supported for plugin authors."
```

---

## Task 5: End-to-end smoke + measurement gate

Verify the new boot flow actually works in a running backoffice, and capture before/after numbers via `measure-ttfe.mjs`.

**Files:**
- No source changes. Measurement output may be saved as an artifact (not committed).

- [ ] **Step 1: Build the full dist-cms**

Run from `src/Umbraco.Web.UI.Client`:

```bash
npm run build:for:cms
```

Expected: full build completes. `dist-cms/manifests-all/index.js` exists. `dist-cms/umbraco-package.json` contains the `manifests-all` importmap entry.

- [ ] **Step 2: Run the Umbraco web project**

Open a new terminal at the repository root:

```bash
dotnet run --project src/Umbraco.Web.UI
```

Wait for the backoffice to be reachable at `https://localhost:44339/umbraco`.

- [ ] **Step 3: Manually smoke-test login**

Open `https://localhost:44339/umbraco` in a fresh Chrome profile. Log in. Verify:
- Login completes without console errors.
- Default landing dashboard renders.
- Open Content section → root tree loads.
- Open a Document → editor loads without errors.

If anything fails, inspect the browser console for missing extension errors. The most likely failure is a manifest that depended on registration order — `registerMany` registers in a single batch, while the old flow registered package-by-package.

- [ ] **Step 4: Capture WAN TTFE measurement**

Run from `src/Umbraco.Web.UI.Client`:

```bash
# One-time: save auth state if not already done
node devops/measure-ttfe.mjs login

# Five WAN-throttled runs against the editor scenario
NETWORK=wan node devops/measure-ttfe.mjs run 5
```

Record the median TTFE.

Compare against the baseline in `~/.claude/projects/-Users-jov-Projects-umbraco-cms-v17/memory/backoffice-perf-optimizations.md`:
- Pre-PR #22896: 5 321 ms WAN
- Post-PR #22896 (current): 4 394 ms WAN
- This change: expect a further improvement on top of 4 394 ms.

- [ ] **Step 5: Capture request-count measurement**

In Chrome DevTools (Network tab) on the editor scenario, count total JS requests at first paint. Compare against memory's pre-change baseline (622 JS requests on the editor). Expect a reduction of ~70+ requests (the two-wave manifest layer collapses).

- [ ] **Step 6: Update the design doc with measurements**

Open `docs/superpowers/specs/2026-05-20-unified-manifests-design.md` and replace the "Measurement gate" section's expectations with the actuals from Steps 4–5. Commit:

```bash
git add docs/superpowers/specs/2026-05-20-unified-manifests-design.md
git commit -m "Docs: Record unified manifests boot measurements (AB#63380)

Captures actual WAN TTFE delta and request-count reduction from the
end-to-end build verifying the unified manifests boot path."
```

- [ ] **Step 7: Update the auto-memory roadmap**

Open `~/.claude/projects/-Users-jov-Projects-umbraco-cms-v17/memory/backoffice-perf-optimizations.md` and move open item #1 (the manifests-unification entry) from "open items" to "shipped", including the measured numbers. Similarly update `~/.claude/projects/-Users-jov-Projects-umbraco-cms-v17/memory/ado-pbi-63091-roadmap.md` to mark #63380 as "Done" if the change is merged.

(Memory file updates do not need a commit — they live outside the repo.)

---

## Verification checklist (run before PR)

- [ ] `npm run lint:errors` passes.
- [ ] `npm run compile` passes (TypeScript).
- [ ] `npm test` passes.
- [ ] `node --test --experimental-strip-types devops/build/vite-plugin-unified-manifests.test.mjs` passes.
- [ ] `npm run build:for:cms` completes; `dist-cms/manifests-all/index.js` exists; `dist-cms/umbraco-package.json` includes the `manifests-all` importmap entry.
- [ ] Manual smoke: login → Content → open document → no console errors.
- [ ] `measure-ttfe.mjs` WAN run shows TTFE improvement vs the 4 394 ms baseline.

## Rollback

If the measurement gate disappoints or smoke testing surfaces an ordering bug:

1. Revert the `app.element.ts` commit (Task 4). This restores the `CORE_PACKAGES` array and the original boot flow.
2. Optionally leave the plugin + build + importmap changes in place (Tasks 1–3); they're additive and harmless if no one imports `@umbraco-cms/backoffice/manifests-all`. Or revert them too for a clean rollback.

The bundle initializer, third-party plugin contract, and per-workspace builds are unchanged, so rollback is a clean revert with no migration concerns.
