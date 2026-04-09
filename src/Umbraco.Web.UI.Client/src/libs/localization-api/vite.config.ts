import { defineConfig } from 'vite';
import { rmSync } from 'fs';
import { getDefaultConfig } from '../../vite-config-base';

const dist = '../../../dist-cms/libs/localization-api';

// delete the unbundled dist folder
rmSync(dist, { recursive: true, force: true });

export default defineConfig({
	...getDefaultConfig({
		dist,
		base: '/umbraco/backoffice/libs/localization-api',
		entry: {
			index: './index.ts',
		},
	}),
});
