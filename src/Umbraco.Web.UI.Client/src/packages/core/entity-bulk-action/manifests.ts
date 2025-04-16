import { manifests as defaultEntityBulkActionManifests } from './default/manifests.js';
import { manifests as duplicateEntityBulkActionManifests } from './common/duplicate-to/manifests.js';
import { manifests as moveToEntityBulkActionManifests } from './common/move-to/manifests.js';
import { manifests as trashEntityBulkActionManifests } from './common/trash/manifests.js';
import { manifests as deleteEntityBulkActionManifests } from './common/bulk-delete/manifests.js';

import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [
	...defaultEntityBulkActionManifests,
	...duplicateEntityBulkActionManifests,
	...moveToEntityBulkActionManifests,
	...trashEntityBulkActionManifests,
	...deleteEntityBulkActionManifests,
];
