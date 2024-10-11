export const manifests: Array<UmbExtensionManifest> = [
  {
    name: "Awesome Entrypoint",
    alias: "my.awesome.entrypoint",
    type: "backofficeEntryPoint",
    js: () => import("./entrypoint"),
  }
];
