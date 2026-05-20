import { readdirSync, existsSync, statSync } from 'node:fs';
import path from 'node:path';
import type { Plugin } from 'vite';

export interface PackageEntry {
	/** Package directory name (e.g. `code-editor`). */
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
		.map((p) => `import { manifests as ${toIdentifier(p.name)} } from "${p.path}";`)
		.join('\n');
	const spreads = packages.map((p) => `\t...${toIdentifier(p.name)},`).join('\n');
	return `${imports}\n\nexport const allManifests = [\n${spreads}\n];\n`;
}

/**
 * Walk a `packages` root directory and return one PackageEntry per child
 * directory that contains a top-level `manifests.ts`. Sorted alphabetically
 * so the generated entry-source (and therefore the `allManifests` spread
 * order) is deterministic across builds and machines.
 */
export function discoverPackages(packagesRoot: string): PackageEntry[] {
	if (!existsSync(packagesRoot)) return [];
	const entries: PackageEntry[] = [];
	for (const dir of readdirSync(packagesRoot).sort()) {
		const abs = path.join(packagesRoot, dir);
		if (!statSync(abs).isDirectory()) continue;
		const manifestPath = path.join(abs, 'manifests.ts');
		if (!existsSync(manifestPath)) continue;
		entries.push({ name: dir, path: manifestPath });
	}
	return entries;
}

const VIRTUAL_ID = 'virtual:unified-manifests';
const PUBLIC_ID = '@umbraco-cms/backoffice/manifests-all';
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
	let discovered: PackageEntry[] = [];
	let cachedSource: string | null = null;
	const refresh = () => {
		discovered = discoverPackages(opts.packagesRoot);
		cachedSource = buildEntrySource(discovered);
	};

	return {
		name: 'umbraco:unified-manifests',
		buildStart() {
			refresh();
		},
		resolveId(id) {
			if (id === VIRTUAL_ID || id === PUBLIC_ID) return RESOLVED_VIRTUAL_ID;
			return null;
		},
		load(id) {
			if (id !== RESOLVED_VIRTUAL_ID) return null;
			if (cachedSource === null) refresh();
			// Track each discovered manifest as a watched file so the dev server's
			// HMR + Rollup's rebuild graph see edits AND additions to the source set.
			// Without this, a brand-new package's manifests.ts wouldn't be in any
			// import chain at startup, so its edits wouldn't fire `handleHotUpdate`.
			for (const entry of discovered) this.addWatchFile(entry.path);
			return cachedSource;
		},
		handleHotUpdate(ctx) {
			// Vite normalises ctx.file to forward-slash separators on all platforms.
			if (!ctx.file.endsWith('/manifests.ts')) return;
			refresh();
			const mod = ctx.server.moduleGraph.getModuleById(RESOLVED_VIRTUAL_ID);
			if (mod) return [mod];
		},
	};
}
