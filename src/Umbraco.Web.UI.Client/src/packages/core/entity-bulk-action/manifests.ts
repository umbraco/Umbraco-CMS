import { manifests as defaultEntityBulkActionManifests } from './default/manifests.js';
import { manifests as duplicateEntityBulkActionManifests } from './common/duplicate-to/manifests.js';
import { manifests as moveToEntityBulkActionManifests } from './common/move-to/manifests.js';
import { manifests as trashEntityBulkActionManifests } from './common/trash/manifests.js';
import type { ManifestTypes, UmbBackofficeManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<ManifestTypes | UmbBackofficeManifestKind> = [
	...defaultEntityBulkActionManifests,
	...duplicateEntityBulkActionManifests,
	...moveToEntityBulkActionManifests,
	...trashEntityBulkActionManifests,
];
