import { defineConfig } from 'vite';

// https://vitejs.dev/config/
export default defineConfig({
	build: {
		lib: {
			entry: ['src/index.ts', 'src/external.ts'],
			formats: ['es'],
		},
    rollupOptions: {
      output: {
        manualChunks: {
          'uui': ['@umbraco-ui/uui'],
          'vendor': ['lit', 'rxjs'],
        },
      }
    },
		target: 'esnext',
		sourcemap: true,
		outDir: '../Umbraco.Cms.StaticAssets/wwwroot/umbraco/login',
		emptyOutDir: true,
	},
});
