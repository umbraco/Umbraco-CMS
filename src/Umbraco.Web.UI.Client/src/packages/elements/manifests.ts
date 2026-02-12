import { manifests as auditLogManifests } from './audit-log/manifests.js';
import { manifests as collectionManifests } from './collection/manifests.js';
import { manifests as entityActionManifests } from './entity-actions/manifests.js';
import { manifests as entityBulkActionManifests } from './entity-bulk-actions/manifests.js';
import { manifests as folderManifests } from './folder/manifests.js';
import { manifests as itemManifests } from './item/manifests.js';
import { manifests as menuManifests } from './menu/manifests.js';
import { manifests as propertyEditorManifests } from './property-editor/manifests.js';
import { manifests as publishingManifests } from './publishing/manifests.js';
import { manifests as recycleBinManifests } from './recycle-bin/manifests.js';
import { manifests as referenceManifests } from './reference/manifests.js';
import { manifests as repositoryManifests } from './repository/manifests.js';
import { manifests as rollbackManifests } from './rollback/manifests.js';
import { manifests as treeManifests } from './tree/manifests.js';
import { manifests as workspaceManifests } from './workspace/manifests.js';
import { manifests as userPermissionsManifests } from './user-permissions/manifests.js';

export const manifests: Array<UmbExtensionManifest> = [
	...auditLogManifests,
	...collectionManifests,
	...entityActionManifests,
	...entityBulkActionManifests,
	...folderManifests,
	...itemManifests,
	...menuManifests,
	...propertyEditorManifests,
	...publishingManifests,
	...recycleBinManifests,
	...referenceManifests,
	...repositoryManifests,
	...rollbackManifests,
	...treeManifests,
	...workspaceManifests,
	...userPermissionsManifests,
];
