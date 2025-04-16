import { defineConfig } from 'vite';
import { rmSync } from 'fs';
import { getDefaultConfig } from '../../vite-config-base';

const dist = '../../../dist-cms/packages/members';

// delete the unbundled dist folder
rmSync(dist, { recursive: true, force: true });

export default defineConfig({
	...getDefaultConfig({
		dist,
		entry: {
			'member/index': 'member/index.ts',
			'member-group/index': 'member-group/index.ts',
			'member-type/index': 'member-type/index.ts',
			'umbraco-package': 'umbraco-package.ts',
			manifests: 'manifests.ts',
		},
	}),
});
