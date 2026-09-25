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
			// The manifests are what the bundle entry loads, so they and everything they reference by
			// value must stay out of the chunk that is meant to load only on entering the subsection.
			// Listing the manifests alone is not enough: manualChunks assigns a module to a chunk
			// regardless of who imports it, so a globalContext or store left off this list still lands
			// in the lazy chunk and the entry then pulls that whole chunk in at boot to reach it.
			eagerModules: ['umbraco-package', 'manifests', 'constants', 'global-context', '.store.'],
		},
	}),
});
