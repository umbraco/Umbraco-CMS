import { defineConfig, PluginOption } from 'vite';
import { viteStaticCopy } from 'vite-plugin-static-copy';
import viteTSConfigPaths from 'vite-tsconfig-paths';
import { build } from 'esbuild';

// Vite plugin that builds the visual editor injected script (IIFE bundle)
// using esbuild. In dev, Vite's own file watcher triggers a rebuild on change
// (no esbuild --watch needed, avoiding file lock conflicts on Windows).
function visualEditorInjectedPlugin(): PluginOption {
	const entry = 'src/apps/visual-editor/injected.ts';
	const outfile = '../Umbraco.Cms.StaticAssets/wwwroot/umbraco/backoffice/apps/visual-editor/injected.js';
	const esbuildOptions = { entryPoints: [entry], bundle: true, format: 'iife' as const, outfile };
	let isServe = false;

	async function rebuild() {
		try {
			await build({ ...esbuildOptions, minify: !isServe });
		} catch (e) {
			// Log but don't crash the dev server on build errors
			console.error('[visual-editor-injected]', e);
		}
	}

	return {
		name: 'visual-editor-injected',
		config(_cfg, { command }) {
			isServe = command === 'serve';
		},
		async buildStart() {
			await rebuild();
		},
		async configureServer(server) {
			server.watcher.add(entry);
			server.watcher.on('change', (path) => {
				if (path.replace(/\\/g, '/').endsWith('apps/visual-editor/injected.ts')) {
					rebuild();
				}
			});
		},
	};
}

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
		],
	}),
	viteTSConfigPaths(),
	visualEditorInjectedPlugin(),
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
