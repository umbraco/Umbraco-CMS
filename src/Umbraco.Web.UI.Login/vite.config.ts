import { defineConfig } from 'vite';
import { resolve } from 'node:path';
import viteTSConfigPaths from 'vite-tsconfig-paths';

// https://vitejs.dev/config/
export default defineConfig({
	plugins: [
		viteTSConfigPaths({
			projects: ['./tsconfig.json', '../Umbraco.Web.UI.Client/tsconfig.json'],
		}),
	],
	server: {
		fs: {
			allow: [import.meta.dirname, resolve(import.meta.dirname, '../Umbraco.Web.UI.Client')],
		},
	},
	build: {
		lib: {
			entry: 'src/index.ts',
			formats: ['es'],
			fileName: 'login',
		},
		rollupOptions: {
			external: [/^@umbraco-cms/],
		},
		target: 'esnext',
		sourcemap: true,
		outDir: '../Umbraco.Cms.StaticAssets/wwwroot/umbraco/login',
		emptyOutDir: true,
	},
});
