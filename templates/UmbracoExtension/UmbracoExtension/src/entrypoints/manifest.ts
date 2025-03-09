export const manifests: Array<UmbExtensionManifest> = [
  {
    name: "Umbraco ExtensionEntrypoint",
    alias: "Umbraco.Extension.Entrypoint",
    type: "backofficeEntryPoint",
    js: () => import("./entrypoint"),
  }
];
