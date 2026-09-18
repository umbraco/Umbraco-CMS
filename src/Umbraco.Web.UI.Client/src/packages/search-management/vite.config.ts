import { defineConfig } from 'vite';
import { rmSync } from 'fs';
import { getDefaultConfig } from '../../vite-config-base';

const dist = '../../../dist-cms/packages/search-management';

// delete the unbundled dist folder
rmSync(dist, { recursive: true, force: true });

export default defineConfig({
	...getDefaultConfig({
		dist,
		lazyChunk: {
			name: 'search-index',
			// Core instantiates globalContext and store extensions at startup (see core/entry-point.ts),
			// and the manifests themselves are what the bundle entry loads, so none of these may end up
			// in the chunk that is meant to load only on entering the subsection.
			eagerModules: ['umbraco-package', 'manifests', 'constants', 'legacy-aliases', 'global-context', '.store.'],
		},
	}),
});
