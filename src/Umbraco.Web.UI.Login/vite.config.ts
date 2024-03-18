import { defineConfig } from 'vite';

// https://vitejs.dev/config/
export default defineConfig({
	build: {
		lib: {
			entry: 'src/index.ts',
      formats: ['es'],
      fileName: 'login'
		},
    rollupOptions: {
      external: [/^@umbraco-cms/],
      output: {
        manualChunks: {
          'uui': ['@umbraco-ui/uui']
        },
      }
    },
		target: 'esnext',
		sourcemap: true,
		outDir: '../Umbraco.Cms.StaticAssets/wwwroot/umbraco/login',
		emptyOutDir: true,
	},
});
