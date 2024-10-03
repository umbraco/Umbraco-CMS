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
});
