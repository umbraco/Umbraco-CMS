import { manifests as emptyRecycleBinManifests } from './empty-recycle-bin/manifests.js';
import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [...emptyRecycleBinManifests];
