import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';
import { manifests as copyManifests } from './copy/manifests.js';
import { manifests as pasteManifests } from './paste/manifests.js';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [...copyManifests, ...pasteManifests];
