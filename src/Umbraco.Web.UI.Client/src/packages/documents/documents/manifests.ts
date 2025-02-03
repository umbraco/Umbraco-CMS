import { manifests as auditLogManifests } from './audit-log/manifests.js';
import { manifests as collectionManifests } from './collection/manifests.js';
import { manifests as entityActionManifests } from './entity-actions/manifests.js';
import { manifests as entityBulkActionManifests } from './entity-bulk-actions/manifests.js';
import { manifests as globalContextManifests } from './global-contexts/manifests.js';
import { manifests as menuManifests } from './menu/manifests.js';
import { manifests as modalManifests } from './modals/manifests.js';
import { manifests as pickerManifests } from './picker/manifests.js';
import { manifests as propertyEditorManifests } from './property-editors/manifests.js';
import { manifests as publishingManifests } from './publishing/manifests.js';
import { manifests as recycleBinManifests } from './recycle-bin/manifests.js';
import { manifests as repositoryManifests } from './repository/manifests.js';
import { manifests as rollbackManifests } from './rollback/manifests.js';
import { manifests as searchProviderManifests } from './search/manifests.js';
import { manifests as trackedReferenceManifests } from './reference/manifests.js';
import { manifests as treeManifests } from './tree/manifests.js';
import { manifests as urlManifests } from './url/manifests.js';
import { manifests as userPermissionManifests } from './user-permissions/manifests.js';
import { manifests as workspaceManifests } from './workspace/manifests.js';

import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [
	...auditLogManifests,
	...collectionManifests,
	...entityActionManifests,
	...entityBulkActionManifests,
	...globalContextManifests,
	...menuManifests,
	...modalManifests,
	...pickerManifests,
	...propertyEditorManifests,
	...publishingManifests,
	...recycleBinManifests,
	...repositoryManifests,
	...rollbackManifests,
	...searchProviderManifests,
	...trackedReferenceManifests,
	...treeManifests,
	...urlManifests,
	...userPermissionManifests,
	...workspaceManifests,
];
