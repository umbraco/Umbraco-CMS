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
					src: 'src/assets/icons/*.js',
					dest: 'icons',
				},
			],
		}),
	],
});
