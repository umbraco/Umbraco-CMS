import { defineConfig } from 'vite';
import { rmSync } from 'fs';
import { getDefaultConfig } from '../../vite-config-base';

const dist = '../../../dist-cms/packages/templating';

// delete the unbundled dist folder
rmSync(dist, { recursive: true, force: true });

export default defineConfig({
	...getDefaultConfig({
		dist,
		entry: {
			'entry-point': 'entry-point.ts',
			'partial-views/index': 'partial-views/index.ts',
			'scripts/index': 'scripts/index.ts',
			'stylesheets/index': 'stylesheets/index.ts',
			'templates/index': 'templates/index.ts',
			'umbraco-package': 'umbraco-package.ts',
			manifests: 'manifests.ts',
		},
	}),
});
