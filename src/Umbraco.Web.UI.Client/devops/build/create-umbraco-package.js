import { createImportMap } from "../importmap/index.js";
import { createPreloadList } from "../importmap/preload.js";
import { writeFileSync, rmSync, readdirSync } from "fs";
import { join } from "path";
import { packageJsonName, packageJsonVersion } from "../package/index.js";

const srcDir = './dist-cms';
const outputModuleList = `${srcDir}/umbraco-package.json`;
const importmap = createImportMap({ rootDir: '/umbraco/backoffice', additionalImports: {} });
importmap.preload = createPreloadList({ rootDir: '/umbraco/backoffice' });

const umbracoPackageJson = {
	name: packageJsonName,
	version: packageJsonVersion,
	extensions: [],
	importmap
};

try {
	rmSync(outputModuleList, { force: true });
	writeFileSync(outputModuleList, JSON.stringify(umbracoPackageJson));
	console.log(`Wrote manifest to ${outputModuleList} (${importmap.preload.length} preload aliases)`);
} catch (e) {
	console.error(`Failed to write manifest to ${outputModuleList}`, e);
	process.exit(1);
}

// Remove per-workspace chunk-graph.json files now that the walker has consumed them.
// These are build-time artifacts only — shipping them would bloat the dist for no runtime use.
function removeChunkGraphs(root) {
	for (const entry of readdirSync(root, { withFileTypes: true })) {
		const full = join(root, entry.name);
		if (entry.isDirectory()) {
			removeChunkGraphs(full);
		} else if (entry.isFile() && entry.name === 'chunk-graph.json') {
			rmSync(full, { force: true });
		}
	}
}
removeChunkGraphs(srcDir);
