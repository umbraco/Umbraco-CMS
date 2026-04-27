import { defineConfig } from 'vite';
import { resolve } from 'node:path';

// https://vitejs.dev/config/
export default defineConfig({
	resolve: {
		tsconfigPaths: true,
	},
	optimizeDeps: {
		exclude: ['@umbraco-ui/uui'],
	},
	server: {
		fs: {
			allow: [resolve(import.meta.dirname, '..')],
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
