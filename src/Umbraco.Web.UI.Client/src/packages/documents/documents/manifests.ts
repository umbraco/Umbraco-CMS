import { manifests as collectionManifests } from './collection/manifests.js';
import { manifests as menuItemManifests } from './menu-item/manifests.js';
import { manifests as repositoryManifests } from './repository/manifests.js';
import { manifests as treeManifests } from './tree/manifests.js';
import { manifests as workspaceManifests } from './workspace/manifests.js';
import { manifests as entityActionManifests } from './entity-actions/manifests.js';
import { manifests as entityBulkActionManifests } from './entity-bulk-actions/manifests.js';
import { manifests as propertyEditorManifests } from './property-editors/manifests.js';
import { manifests as userPermissionManifests } from './user-permissions/manifests.js';
import { manifests as recycleBinManifests } from './recycle-bin/manifests.js';

export const manifests = [
	...collectionManifests,
	...menuItemManifests,
	...treeManifests,
	...repositoryManifests,
	...workspaceManifests,
	...entityActionManifests,
	...entityBulkActionManifests,
	...propertyEditorManifests,
	...userPermissionManifests,
	...recycleBinManifests,
];
