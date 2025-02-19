import { manifests as bulkTrashManifests } from './entity-actions/bulk-trash/manifests.js';
import { manifests as collectionManifests } from './collection/manifests.js';
import { manifests as deleteManifests } from './entity-actions/delete/manifests.js';
import { manifests as trashManifests } from './entity-actions/trash/manifests.js';
import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [
	...bulkTrashManifests,
	...collectionManifests,
	...deleteManifests,
	...trashManifests,
];
