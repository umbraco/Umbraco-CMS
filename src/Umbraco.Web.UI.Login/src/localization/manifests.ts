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
    weight: 0,
    js: () => import("./lang/da-dk.js"),
    meta: {
      culture: "da-dk",
    }
  },
  {
    type: "localization",
    alias: "Umb.Auth.Localization.DeDe",
    name: "German (DE)",
    weight: 0,
    js: () => import("./lang/de-de.js"),
    meta: {
      culture: "de-de",
    }
  },
  {
    type: "localization",
    alias: "Umb.Auth.Localization.NlNl",
    name: "Dutch (NL)",
    weight: 0,
    js: () => import("./lang/nl-nl.js"),
    meta: {
      culture: "nl-nl",
    }
  },
]
