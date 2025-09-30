import { defineConfig } from 'vite';

import { plugins } from './vite.config';

export default defineConfig({
	build: {
		lib: {
			entry: 'src/apps/app/app.element.ts',
			formats: ['es'],
			fileName: 'main',
		},
		rollupOptions: {
			external: [/^@umbraco-cms\/backoffice\//],
		},
		outDir: '../Umbraco.Cms.StaticAssets/wwwroot/umbraco/backoffice',
		emptyOutDir: false,
		sourcemap: true,
	},
	base: '/umbraco/backoffice/',
	mode: 'production',
	plugins: [...plugins],
});
