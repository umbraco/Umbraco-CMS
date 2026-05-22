import type { Plugin } from 'vite';

/**
 * Vite plugin that emits a `chunk-graph.json` alongside the workspace's
 * `dist` output. Captures the static and dynamic imports for every emitted
 * chunk so the boot-eager preload list can be derived at build time.
 *
 * Used by {@link ../devops/importmap/preload.js} to compute the alias list
 * written to `importmap.preload` in `dist-cms/umbraco-package.json`.
 */
export const chunkGraphPlugin = (): Plugin => ({
	name: 'umb-chunk-graph',
	generateBundle(_, bundle) {
		const graph: Record<
			string,
			{
				isEntry: boolean;
				facadeModuleId: string | null;
				imports: string[];
				dynamicImports: string[];
			}
		> = {};

		for (const [fileName, chunk] of Object.entries(bundle)) {
			if (chunk.type !== 'chunk') continue;
			graph[fileName] = {
				isEntry: chunk.isEntry,
				facadeModuleId: chunk.facadeModuleId,
				imports: chunk.imports,
				dynamicImports: chunk.dynamicImports,
			};
		}

		this.emitFile({
			type: 'asset',
			fileName: 'chunk-graph.json',
			source: JSON.stringify(graph, null, 2),
		});
	},
});
