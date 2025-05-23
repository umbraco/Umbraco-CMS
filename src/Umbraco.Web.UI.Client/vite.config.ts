import { defineConfig, PluginOption } from 'vite';
import { viteStaticCopy } from 'vite-plugin-static-copy';
import viteTSConfigPaths from 'vite-tsconfig-paths';

export const plugins: PluginOption[] = [
	viteStaticCopy({
		targets: [
			{
				src: 'public-assets/App_Plugins/*.js',
				dest: 'App_Plugins',
			},
			{
				src: 'public-assets/App_Plugins/custom-bundle-package/*.js',
				dest: 'App_Plugins/custom-bundle-package',
			},
			{
				src: 'src/css/*.css',
				dest: 'umbraco/backoffice/css',
			},
			{
				src: 'node_modules/@umbraco-ui/uui-css/dist/uui-css.css',
				dest: 'umbraco/backoffice/css',
			},
			{
				src: 'node_modules/@umbraco-ui/uui-css/assets/fonts/*',
				dest: 'umbraco/backoffice/assets/fonts',
			},
			{
				src: 'src/assets/*',
				dest: 'umbraco/backoffice/assets',
			},
			{
				src: 'src/mocks/handlers/backoffice/assets/*',
				dest: 'umbraco/backoffice/assets',
			},
			{
				src: 'node_modules/msw/lib/iife/**/*',
				dest: 'umbraco/backoffice/msw',
			},
			{
				src: 'node_modules/monaco-editor/esm/**/*',
				dest: 'umbraco/backoffice/monaco-editor/vs',
			},
		],
	}),
	viteTSConfigPaths(),
];

// https://vitejs.dev/config/
export default defineConfig({
	build: {
		sourcemap: true,
		rollupOptions: {
			input: {
				main: new URL('index.html', import.meta.url).pathname, // Vite should only load the main index.html file
			},
		},
	},
	plugins,
});
