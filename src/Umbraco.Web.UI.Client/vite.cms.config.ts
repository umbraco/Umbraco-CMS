import { defineConfig } from 'vite';
import { viteStaticCopy } from 'vite-plugin-static-copy';

import { plugins } from './vite.config';

export default defineConfig({
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
	mode: 'production',
	plugins: [
		...plugins,
		viteStaticCopy({
			targets: [
				{
					src: 'types/umbraco-package-schema.json',
					dest: '../../../../Umbraco.Web.UI.New',
				},
			],
		}),
	],
});
