import { defineConfig } from 'vite';
import { viteStaticCopy } from 'vite-plugin-static-copy';

// https://vitejs.dev/config/
export default defineConfig({
	build: {
		sourcemap: true,
	},
	plugins: [
		viteStaticCopy({
			targets: [
				{
					src: 'public-assets/icons/*.js',
					dest: 'icons',
				},
			],
		}),
	],
});
