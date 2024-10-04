import { defineConfig } from "vite";

export default defineConfig({
  build: {
    lib: {
      entry: "src/entrypoint.ts", // Entrypoint file (registers other manifests)
      formats: ["es"],
      fileName: "PROJECT_NAME_KEBABCASE_FOR_NPM",
    },
    outDir: "../wwwroot/App_Plugins/UmbracoExtension", // your web component will be saved in this location
    emptyOutDir: true,
    sourcemap: true,
    rollupOptions: {
      external: [/^@umbraco/],
    },
  },

  // Used to allow serving SVG, images & other assets from /Client/public
  // Such as <img src="/logo.jpg" />
  // or
  // import imageUrl from '/logo.jpg';
  // <img src=${imageUrl} />
  base: '/App_Plugins/UmbracoExtension', 
});
