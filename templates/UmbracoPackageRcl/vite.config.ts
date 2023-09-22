import { defineConfig } from "vite";

export default defineConfig({
  build: {
    lib: {
      entry: ["src/property-editor/my-property-editor-ui.ts", "src/dashboard/my-welcome-dashboard.ts"], // your web component source file
      formats: ["es"],
    },
    outDir: "wwwroot/dist", // your web component will be saved in this location
    sourcemap: true,
    emptyOutDir: true,
    rollupOptions: {
      external: [/^@umbraco/],
    },
  },
});
