import { readdirSync, statSync } from 'node:fs';
import path from 'node:path';
import type { Plugin } from 'vite';

export interface PackageEntry {
	/** Package directory name (e.g. `code-editor`). */
	name: string;
	/** Absolute or virtual-resolvable path to the package's manifests.ts. */
	path: string;
}

/** Convert a kebab-case package directory name into a JS identifier. */
function toIdentifier(pkgDir: string): string {
	return pkgDir.replace(/-([a-z])/g, (_, c) => c.toUpperCase());
}

/**
 * Generate the virtual entry's TypeScript source. Pure function: takes the
 * resolved package list and emits one import per package plus a spread-array
 * named `allManifests`.
 */
export function buildEntrySource(packages: PackageEntry[]): string {
	const idented = packages.map((p) => ({ id: toIdentifier(p.name), path: p.path }));
	const imports = idented.map((p) => `import { manifests as ${p.id} } from "${p.path}";`).join('\n');
	const spreads = idented.map((p) => `\t...${p.id},`).join('\n');
	return `${imports}\n\nexport const allManifests = [\n${spreads}\n];\n`;
}

/**
 * Walk a `packages` root directory and return one PackageEntry per child
 * directory that contains a top-level `manifests.ts`. Sorted alphabetically
 * so the generated entry-source (and therefore the `allManifests` spread
 * order) is deterministic across builds and machines.
 */
function discoverPackages(packagesRoot: string): PackageEntry[] {
	let dirents;
	try {
		dirents = readdirSync(packagesRoot, { withFileTypes: true });
	} catch {
		return [];
	}
	const entries: PackageEntry[] = [];
	for (const dirent of dirents.sort((a, b) => a.name.localeCompare(b.name))) {
		if (!dirent.isDirectory()) continue;
		const manifestPath = path.join(packagesRoot, dirent.name, 'manifests.ts');
		try {
			statSync(manifestPath);
		} catch {
			continue;
		}
		entries.push({ name: dirent.name, path: manifestPath });
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
	let cache: { entries: PackageEntry[]; source: string } | null = null;
	const rediscover = () => {
		const entries = discoverPackages(opts.packagesRoot);
		cache = { entries, source: buildEntrySource(entries) };
	};

	return {
		name: 'umbraco:unified-manifests',
		buildStart() {
			rediscover();
		},
		resolveId(id) {
			if (id === VIRTUAL_ID || id === PUBLIC_ID) return RESOLVED_VIRTUAL_ID;
			return null;
		},
		load(id) {
			if (id !== RESOLVED_VIRTUAL_ID) return null;
			if (cache === null) rediscover();
			// Track every discovered manifest so the dev server sees edits AND
			// additions to the source set. Without addWatchFile, a brand-new
			// package's manifests.ts wouldn't be in any import chain at startup
			// and its edits wouldn't fire handleHotUpdate.
			for (const entry of cache!.entries) this.addWatchFile(entry.path);
			return cache!.source;
		},
		handleHotUpdate(ctx) {
			// Vite normalises ctx.file to forward-slash separators on all platforms.
			if (!ctx.file.endsWith('/manifests.ts')) return;
			const known = cache?.entries.some((e) => e.path === ctx.file) ?? false;
			const prevSource = cache?.source ?? null;
			if (known) {
				// Same set of packages — only the content may have shifted identifiers.
				if (cache) cache.source = buildEntrySource(cache.entries);
			} else {
				// New manifests.ts appeared — re-walk the directory.
				rediscover();
			}
			if (cache?.source === prevSource) return;
			const mod = ctx.server.moduleGraph.getModuleById(RESOLVED_VIRTUAL_ID);
			if (mod) return [mod];
		},
	};
}
