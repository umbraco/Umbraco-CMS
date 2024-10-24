export const manifests: Array<UmbExtensionManifest> = [
  {
    name: "UmbracoExtension Entrypoint",
    alias: "PROJECT_NAME_KEBABCASE_FOR_NPM.entrypoint",
    type: "backofficeEntryPoint",
    js: () => import("./entrypoint"),
  }
];
