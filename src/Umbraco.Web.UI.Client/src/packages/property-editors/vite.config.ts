import { defineConfig } from 'vite';
import { rmSync } from 'fs';
import { getDefaultConfig } from '../../vite-config-base';

const dist = '../../../dist-cms/packages/property-editors';

// delete the unbundled dist folder
rmSync(dist, { recursive: true, force: true });

export default defineConfig({
	...getDefaultConfig({
		dist,
		entry: {
			'entry-point': 'entry-point.ts',
			'umbraco-package': 'umbraco-package.ts',
			manifests: 'manifests.ts',
			'content-picker/index': './content-picker/index.ts',
			'entity-data-picker/index': './entity-data-picker/index.ts',
		},
	}),
});
