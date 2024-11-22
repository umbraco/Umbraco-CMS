import { defineConfig } from 'vite';
import { rmSync } from 'fs';
import { getDefaultConfig } from '../../vite-config-base';

const dist = '../../../dist-cms/packages/documents';

// delete the unbundled dist folder
rmSync(dist, { recursive: true, force: true });

export default defineConfig({
	...getDefaultConfig({
		dist,
		entry: {
			'document-blueprints/index': 'document-blueprints/index.ts',
			'document-types/index': 'document-types/index.ts',
			'documents/index': 'documents/index.ts',
			manifests: 'manifests.ts',
			'umbraco-package': 'umbraco-package.ts',
		},
	}),
});
