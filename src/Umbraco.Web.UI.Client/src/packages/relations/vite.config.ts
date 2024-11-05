import { defineConfig } from 'vite';
import { rmSync } from 'fs';
import { getDefaultConfig } from '../../vite-config-base';

const dist = '../../../dist-cms/packages/relations';

// delete the unbundled dist folder
rmSync(dist, { recursive: true, force: true });

export default defineConfig({
	...getDefaultConfig({
		dist,
		entry: {
			'relation-types/index': 'relation-types/index.ts',
			'relations/index': 'relations/index.ts',
			manifests: 'manifests.ts',
			'umbraco-package': 'umbraco-package.ts',
		},
	}),
});
