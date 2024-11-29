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
				src: 'src/assets/*',
				dest: 'umbraco/backoffice/assets',
			},
			{
				src: 'node_modules/tinymce/**/*',
				dest: 'umbraco/backoffice/tinymce',
			},
			{
				src: 'node_modules/tinymce-i18n/langs6/**/*',
				dest: 'umbraco/backoffice/tinymce/langs',
			},
			{
				src: 'node_modules/msw/lib/iife/**/*',
				dest: 'umbraco/backoffice/msw',
			},
			{
				src: 'node_modules/monaco-editor/esm/**/*',
				dest: 'umbraco/backoffice/monaco-editor/esm',
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
