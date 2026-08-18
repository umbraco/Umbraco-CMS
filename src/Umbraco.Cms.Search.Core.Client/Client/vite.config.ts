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
      allow: [import.meta.dirname, resolve(import.meta.dirname, '../../Umbraco.Web.UI.Client')],
    },
  },
  build: {
    lib: {
      entry: {
        'search-bundle': 'src/bundle/search-bundle.ts',
        'search-global': 'src/global/search-global.ts',
        'search-settings': 'src/settings/search-settings.ts',
      }, // Bundle registers one or more manifests
      formats: ['es'],
    },
    outDir: '../wwwroot/App_Plugins/UmbracoSearch', // your web component will be saved in this location
    emptyOutDir: true,
    sourcemap: true,
    rollupOptions: {
      external: [/^@umbraco/],
    },
  },
});
