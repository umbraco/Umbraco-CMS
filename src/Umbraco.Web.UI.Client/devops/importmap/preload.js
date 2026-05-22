import { readFileSync, existsSync, readdirSync } from 'fs';
import { dirname, join, posix, relative, sep } from 'path';
import { packageJsonExports, packageJsonName } from '../package/index.js';

/**
 * Computes the list of importmap aliases that are statically reachable from
 * the backoffice entry script (`apps/app/app.element.js`) and should be
 * preloaded via `<link rel="modulepreload">` in the backoffice index HTML.
 *
 * Static reach matters because the browser would otherwise discover these
 * chunks serially as each parent chunk parses (one round-trip per level of
 * indirection). Preloading collapses the waterfall.
 *
 * Dynamic imports (the per-package umbraco-package.js and manifests.js
 * bootstrap chain) are intentionally NOT followed. They are not aliased today
 * and load lazily; the browser's own modulepreload dependency walker handles
 * the subgraphs hanging off the aliased chunks we do declare.
 *
 * Reads `chunk-graph.json` files emitted by the `chunkGraphPlugin` from each
 * workspace's Vite build. Entry-script imports are discovered by parsing the
 * top-level TS source — the apps/app folder is TSC-compiled, not Vite-built.
 */

const POSIX_DIST = 'dist-cms';
const ROOT_DIR = '/umbraco/backoffice';
const ENTRY_TS = 'src/apps/app/app.element.ts';
const EXTERNAL_PREFIX = `${packageJsonName}/`;
const IMPORT_PATTERN = /(?:^|\s|;)import\s+(?:[^"']*?\sfrom\s+)?["']([^"']+)["']/g;

/**
 * @returns {string[]} alias keys (e.g., `@umbraco-cms/backoffice/auth`) sorted alphabetically
 */
export const createPreloadList = ({ rootDir = ROOT_DIR } = {}) => {
	const aliasToUrl = buildAliasMap(rootDir);
	const urlToAlias = invert(aliasToUrl);
	const chunkGraphs = loadChunkGraphs();

	// Seed: external alias imports parsed directly from the TSC-compiled entry source.
	const seedAliases = parseEntryAliases();
	const visitedAliases = new Set();
	const visitedFiles = new Set();
	const queue = [];

	for (const alias of seedAliases) {
		if (!aliasToUrl[alias]) continue;
		queue.push({ kind: 'alias', value: alias });
	}

	while (queue.length) {
		const item = queue.shift();
		if (item.kind === 'alias') {
			if (visitedAliases.has(item.value)) continue;
			visitedAliases.add(item.value);
			const url = aliasToUrl[item.value];
			if (!url) continue;
			const workspaceDir = workspaceDirForUrl(url, rootDir);
			const chunkFile = chunkFileForUrl(url, workspaceDir, rootDir);
			if (workspaceDir && chunkFile) {
				queue.push({ kind: 'chunk', workspace: workspaceDir, file: chunkFile });
			}
		} else {
			const key = `${item.workspace}::${item.file}`;
			if (visitedFiles.has(key)) continue;
			visitedFiles.add(key);
			const graph = chunkGraphs[item.workspace];
			const node = graph?.[item.file];
			if (!node) continue;
			for (const imp of node.imports) {
				if (imp.startsWith(EXTERNAL_PREFIX)) {
					queue.push({ kind: 'alias', value: imp });
				} else {
					// Same-workspace chunk — Rollup's `chunk.imports` lists peer chunk files.
					queue.push({ kind: 'chunk', workspace: item.workspace, file: imp });
				}
			}
		}
	}

	// Filter: only aliases whose chunks were reached, not the seed itself unless visited.
	// (Each seed enters visitedAliases on dequeue.)
	return [...visitedAliases].sort();
};

const buildAliasMap = (rootDir) => {
	/** @type {Record<string, string>} */
	const map = {};
	for (const [key, value] of Object.entries(packageJsonExports || {})) {
		if (typeof value !== 'string' || !value.endsWith('.js')) continue;
		const moduleName = key.replace(/^\.\//, '');
		const alias = `${packageJsonName}/${moduleName}`;
		const url = value.replace(/^\.\/dist-cms/, rootDir);
		map[alias] = url;
	}
	return map;
};

const invert = (map) => {
	/** @type {Record<string, string>} */
	const inv = {};
	for (const [k, v] of Object.entries(map)) inv[v] = k;
	return inv;
};

const loadChunkGraphs = () => {
	/** @type {Record<string, Record<string, { isEntry: boolean; imports: string[]; dynamicImports: string[] }>>} */
	const graphs = {};
	for (const file of findChunkGraphs(POSIX_DIST)) {
		const dir = dirname(file).split(sep).join(posix.sep);
		try {
			graphs[dir] = JSON.parse(readFileSync(file, 'utf-8'));
		} catch (e) {
			console.warn(`[preload] skipping unreadable chunk-graph at ${file}:`, e.message);
		}
	}
	return graphs;
};

function* findChunkGraphs(root) {
	if (!existsSync(root)) return;
	for (const entry of readdirSync(root, { withFileTypes: true })) {
		const full = join(root, entry.name);
		if (entry.isDirectory()) {
			yield* findChunkGraphs(full);
		} else if (entry.isFile() && entry.name === 'chunk-graph.json') {
			yield full;
		}
	}
}

const workspaceDirForUrl = (url, rootDir) => {
	// url looks like `/umbraco/backoffice/packages/auth/index.js` → workspace dist is
	// `dist-cms/packages/auth`. The chunk file is `index.js`.
	if (!url.startsWith(rootDir)) return null;
	const rel = url.slice(rootDir.length).replace(/^\//, '');
	// Walk up from the file path checking for chunk-graph.json presence.
	const segments = rel.split('/');
	for (let i = segments.length - 1; i >= 1; i--) {
		const candidate = `${POSIX_DIST}/${segments.slice(0, i).join('/')}`;
		if (existsSync(join(candidate, 'chunk-graph.json'))) {
			return candidate;
		}
	}
	return null;
};

const chunkFileForUrl = (url, workspaceDir, rootDir) => {
	if (!workspaceDir) return null;
	const rel = url.slice(rootDir.length).replace(/^\//, '');
	const workspaceRel = workspaceDir.replace(`${POSIX_DIST}/`, '');
	if (!rel.startsWith(`${workspaceRel}/`)) return null;
	return rel.slice(workspaceRel.length + 1);
};

const parseEntryAliases = () => {
	const path = relative(process.cwd(), ENTRY_TS);
	const source = readFileSync(path, 'utf-8');
	const found = new Set();
	for (const match of source.matchAll(IMPORT_PATTERN)) {
		const spec = match[1];
		if (spec.startsWith(EXTERNAL_PREFIX)) found.add(spec);
	}
	return [...found];
};
