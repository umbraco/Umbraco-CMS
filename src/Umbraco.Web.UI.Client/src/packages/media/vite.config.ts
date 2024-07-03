import { defineConfig } from 'vite';
import { rmSync } from 'fs';
import { getDefaultConfig } from '../../vite-config-base';

const dist = '../../../dist-cms/packages/media';

// delete the unbundled dist folder
rmSync(dist, { recursive: true, force: true });

export default defineConfig({
	...getDefaultConfig({
		dist,
		entry: {
			'media-type/index': 'media-type/index.ts',
			'media/index': 'media/index.ts',
			'imaging/index': 'imaging/index.ts',
			manifests: 'manifests.ts',
			'umbraco-package': 'umbraco-package.ts',
		},
	}),
});
