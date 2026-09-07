import { defineConfig } from 'vite';
import { resolve } from 'node:path';
import viteTSConfigPaths from 'vite-tsconfig-paths';

export default defineConfig({
  plugins: [
    viteTSConfigPaths({
      projects: ['./tsconfig.json', '../../Umbraco.Web.UI.Client/tsconfig.json'],
    }),
  ],
  server: {
    fs: {
      allow: [
        import.meta.dirname,
        resolve(import.meta.dirname, '../../Umbraco.Web.UI.Client'),
        resolve(import.meta.dirname, '../../Umbraco.Cms.Search.Core.Client/Client'),
      ],
    },
  },
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
