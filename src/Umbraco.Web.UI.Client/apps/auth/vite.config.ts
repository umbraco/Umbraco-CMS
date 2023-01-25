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
		sourcemap: true,
	},
	server: {
		fs: {
			// Allow serving files from the global node_modules folder
			allow: ['.', '../../node_modules'],
		},
	},
	plugins: [viteTSConfigPaths()],
});
