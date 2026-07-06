import { defineConfig } from 'vite';

export default defineConfig({
  build: {
    lib: {
      entry: 'src/examine-bundle.ts',
      formats: ['es'],
      fileName: () => 'examine-bundle.js',
    },
    outDir: '../wwwroot/App_Plugins/UmbracoSearchExamine',
    emptyOutDir: true,
    sourcemap: true,
    rollupOptions: {
      external: [/^@umbraco/],
    },
  },
});
