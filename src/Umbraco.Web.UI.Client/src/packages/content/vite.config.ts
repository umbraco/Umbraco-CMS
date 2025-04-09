import { defineConfig } from 'vite';
import { rmSync } from 'fs';
import { getDefaultConfig } from '../../vite-config-base';

const dist = '../../../dist-cms/packages/content';

// delete the unbundled dist folder
rmSync(dist, { recursive: true, force: true });

export default defineConfig({
	...getDefaultConfig({
		dist,
		entry: {
			'content-type/index': './content-type/index.ts',
			'content/index': './content/index.ts',
			'umbraco-package': 'umbraco-package.ts',
			manifests: 'manifests.ts',
		},
	}),
});
