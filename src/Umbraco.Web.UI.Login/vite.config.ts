import { defineConfig } from 'vite';

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
			output: {
				manualChunks: {
					uui: ['@umbraco-ui/uui'],
				},
			},
		},
		outDir: '../Umbraco.Cms.StaticAssets/wwwroot/umbraco/login',
		emptyOutDir: true,
	},
});
