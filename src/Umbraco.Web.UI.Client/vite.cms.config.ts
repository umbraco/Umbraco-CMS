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
		outDir: '../Umbraco.Cms.StaticAssets/wwwroot/umbraco',
		emptyOutDir: true,
		sourcemap: true,
	},
	base: '/umbraco/',
	mode: 'production'
});
