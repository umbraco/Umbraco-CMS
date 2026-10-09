import { defineConfig } from 'vite';
import { viteStaticCopy } from 'vite-plugin-static-copy';

export default defineConfig({
	server: {
		open: '/icon-manager.html',
		fs: {
			// Allow reading icon-dictionary.json from the parent backoffice source tree
			// (../../src/packages/core/icon-registry/icon-dictionary.json).
			allow: ['..', '../..'],
		},
	},
	plugins: [
		viteStaticCopy({
			targets: [
				{
					src: 'node_modules/@umbraco-ui/uui-css/dist/uui-css.css',
					dest: 'umbraco/backoffice/css',
				},
				{
					src: 'node_modules/@umbraco-ui/uui-css/assets/fonts/*',
					dest: 'umbraco/backoffice/assets/fonts',
				},
			],
		}),
	],
	build: {
		rollupOptions: {
			input: {
				'icon-manager': new URL('icon-manager.html', import.meta.url).pathname,
			},
		},
	},
});
