import { ManifestBase } from "@umbraco-cms/backoffice/extension-api";

import { manifests as localizationManifests } from "./localization/manifests";

export const manifests: ManifestBase[] = [
  ...localizationManifests
];
