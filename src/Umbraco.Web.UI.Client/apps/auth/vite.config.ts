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
	plugins: [viteTSConfigPaths()],
});
