import { defineConfig } from 'vite';
import viteTSConfigPaths from 'vite-tsconfig-paths';

// https://vitejs.dev/config/
export default defineConfig({
	build: {
		lib: {
			entry: 'src/index.ts',
			formats: ['es'],
			fileName: 'main',
		},
		target: 'esnext',
		sourcemap: true,
		rollupOptions: {
			external: [/^@umbraco-cms\/backoffice\//],
		},
		outDir: '../../../Umbraco.Cms.StaticAssets/wwwroot/umbraco/login',
		emptyOutDir: true,
	},
	server: {
		fs: {
			// Allow serving files from the global node_modules folder
			allow: ['.', '../../node_modules'],
		},
	},
	plugins: [viteTSConfigPaths()],
});
