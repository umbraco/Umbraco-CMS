import { defineConfig } from 'vite';
import { viteStaticCopy } from 'vite-plugin-static-copy';
import viteTSConfigPaths from 'vite-tsconfig-paths';

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
				{
					src: 'public-assets/App_Plugins/*.js',
					dest: 'App_Plugins',
				},
			],
		}),
		viteTSConfigPaths(),
	],
});
