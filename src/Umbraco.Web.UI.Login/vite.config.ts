import { defineConfig } from 'vite';
import viteTSConfigPaths from 'vite-tsconfig-paths';

// https://vitejs.dev/config/
export default defineConfig({
  plugins: [viteTSConfigPaths()],
	build: {
		lib: {
			entry: 'src/index.ts',
      formats: ['es'],
      fileName: 'login'
		},
    rollupOptions: {
      external: [/^@umbraco-cms/]
    },
		target: 'esnext',
		sourcemap: true,
		outDir: '../Umbraco.Cms.StaticAssets/wwwroot/umbraco/login',
		emptyOutDir: true,
	},
});
