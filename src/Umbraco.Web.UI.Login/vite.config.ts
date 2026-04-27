import { defineConfig } from 'vite';

// https://vitejs.dev/config/
export default defineConfig({
	resolve: {
		tsconfigPaths: true,
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
