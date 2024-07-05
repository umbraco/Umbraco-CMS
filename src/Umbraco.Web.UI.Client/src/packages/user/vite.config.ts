import { defineConfig } from 'vite';
import { rmSync } from 'fs';
import { getDefaultConfig } from '../../vite-config-base';

const dist = '../../../dist-cms/packages/user';

// delete the unbundled dist folder
rmSync(dist, { recursive: true, force: true });

export default defineConfig({
	...getDefaultConfig({
		dist,
		entry: {
			'user-group/index': 'user-group/index.ts',
			'user-permission/index': 'user-permission/index.ts',
			'user/index': 'user/index.ts',
			'current-user/index': 'current-user/index.ts',
			manifests: 'manifests.ts',
			'umbraco-package': 'umbraco-package.ts',
		},
	}),
});
