import type { ManifestLocalization } from "@umbraco-cms/backoffice/extension-registry";

export const manifests: ManifestLocalization[] = [
  {
    type: "localization",
    alias: "Umb.Auth.Localization.EnUs",
    name: "English (US)",
    weight: 0,
    js: () => import("./lang/en-us.js"),
    meta: {
      culture: "en-US",
    }
  }
]
