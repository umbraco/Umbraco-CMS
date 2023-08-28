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
				src: 'public-assets/App_Plugins/custom-bundle-package/*.js',
				dest: 'App_Plugins/custom-bundle-package',
			},
			{
				src: 'src/assets/*.svg',
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
