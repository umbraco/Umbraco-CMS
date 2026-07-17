import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';
import { manifests as defaultManifests } from './default/manifests.js';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [...defaultManifests];
