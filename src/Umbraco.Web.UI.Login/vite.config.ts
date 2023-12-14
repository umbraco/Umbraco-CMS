import { defineConfig } from 'vite';

// https://vitejs.dev/config/
export default defineConfig({
	build: {
		lib: {
			entry: 'src/index.ts',
      formats: ['iife'],
      name: 'umblogin',
      fileName: 'login'
		},
    rollupOptions: {
      external: [/^@umbraco/],
      output: {
        globals: {
          '@umbraco-ui/uui': 'uui',
        },
      }
    },
		target: 'esnext',
		sourcemap: true,
		outDir: '../Umbraco.Cms.StaticAssets/wwwroot/umbraco/login',
		emptyOutDir: true,
	},
});
