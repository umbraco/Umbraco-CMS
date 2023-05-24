import { defineConfig, PluginOption } from 'vite';
import { viteStaticCopy } from 'vite-plugin-static-copy';
import viteTSConfigPaths from 'vite-tsconfig-paths';

export const plugins: PluginOption[] = [
	viteStaticCopy({
		targets: [
			{
				src: 'src/shared/icon-registry/icons/*.js',
				dest: 'icons',
			},
			{
				src: 'public-assets/App_Plugins/*.js',
				dest: 'App_Plugins',
			},
			{
				src: 'src/assets/*',
				dest: 'assets/umbraco/backoffice',
			},
		],
	}),
	viteTSConfigPaths(),
];

// https://vitejs.dev/config/
export default defineConfig({
	build: {
		sourcemap: true,
	},
	plugins,
});
