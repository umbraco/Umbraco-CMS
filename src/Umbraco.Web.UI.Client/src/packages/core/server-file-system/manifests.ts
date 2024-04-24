import { manifests as renameEntityActionManifests } from '../server-file-system/rename/manifests.js';
import type { ManifestTypes, UmbBackofficeManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<ManifestTypes | UmbBackofficeManifestKind> = [...renameEntityActionManifests];
