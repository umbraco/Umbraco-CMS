import { manifests as renameEntityActionManifests } from '../server-file-system/rename/manifests.js';
import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [...renameEntityActionManifests];
