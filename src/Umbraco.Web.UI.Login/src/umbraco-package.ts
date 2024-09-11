import type { ManifestBundle } from "@umbraco-cms/backoffice/extension-api";

export const name = 'Umbraco.Auth';
export const version = '1.0.0';
export const extensions: Array<ManifestBundle> = [
  {
    name: 'Auth Bundle',
    alias: 'Umb.Auth.Bundle',
    type: 'bundle',
    js: () => import('./manifests.js'),
  },
];
