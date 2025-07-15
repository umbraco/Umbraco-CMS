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
			'entry-point': 'entry-point.ts',
			'imaging/index': 'imaging/index.ts',
			'dropzone/index': 'dropzone/index.ts',
			'media-types/index': 'media-types/index.ts',
			'media/index': 'media/index.ts',
			'umbraco-package': 'umbraco-package.ts',
			manifests: 'manifests.ts',
		},
	}),
});
