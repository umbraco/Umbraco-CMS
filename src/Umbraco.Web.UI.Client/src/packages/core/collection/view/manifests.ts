import { manifests as cardManifests } from './card/manifests.js';
import { manifests as refManifests } from './ref/manifests.js';
import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [...cardManifests, ...refManifests];
