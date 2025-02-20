import { defineConfig } from 'vite';
import { rmSync } from 'fs';
import { getDefaultConfig } from '../../vite-config-base';

const dist = '../../../dist-cms/packages/block';

// delete the unbundled dist folder
rmSync(dist, { recursive: true, force: true });

export default defineConfig({
	...getDefaultConfig({
		dist,
		entry: {
			'block/index': 'block/index.ts',
			'block-custom-view/index': 'block-custom-view/index.ts',
			'block-grid/index': 'block-grid/index.ts',
			'block-list/index': 'block-list/index.ts',
			'block-rte/index': 'block-rte/index.ts',
			'block-type/index': 'block-type/index.ts',
			manifests: 'manifests.ts',
			'umbraco-package': 'umbraco-package.ts',
		},
	}),
});
