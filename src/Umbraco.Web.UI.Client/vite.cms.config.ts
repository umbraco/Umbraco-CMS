import { defineConfig } from 'vite';

import config from './vite.config';

export default defineConfig({
	...config,
	build: {
		lib: {
			entry: 'src/app.ts',
			formats: ['es'],
			fileName: 'main',
		},
		rollupOptions: {
			external: [/^@umbraco-cms\/backoffice\//]
		},
		outDir: '../Umbraco.Cms.StaticAssets/wwwroot/umbraco/backoffice',
		emptyOutDir: false,
		sourcemap: true,
	},
	base: '/umbraco/backoffice/',
	mode: 'production'
});
