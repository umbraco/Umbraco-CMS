import { manifests as classicManifests } from './classic/manifests.js';
import { manifests as cardManifests } from './card/manifests.js';
import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [...classicManifests, ...cardManifests];
