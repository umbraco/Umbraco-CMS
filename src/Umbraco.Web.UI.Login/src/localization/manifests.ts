import type { ManifestLocalization } from "@umbraco-cms/backoffice/extension-registry";

export const manifests: ManifestLocalization[] = [
  {
    type: "localization",
    alias: "Umb.Auth.Localization.EnUs",
    name: "English (US)",
    weight: 0,
    js: () => import("./lang/en-us.js"),
    meta: {
      culture: "en-us",
    }
  },
  {
    type: "localization",
    alias: "Umb.Auth.Localization.DaDk",
    name: "Danish (DK)",
    weight: 1,
    js: () => import("./lang/da-dk.js"),
    meta: {
      culture: "da-dk",
    }
  }
]
