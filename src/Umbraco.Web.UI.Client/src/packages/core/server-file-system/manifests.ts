import { manifests as renameEntityActionManifests } from '../server-file-system/rename/manifests.js';
import type { ManifestTypes, UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<ManifestTypes | UmbExtensionManifestKind> = [...renameEntityActionManifests];
