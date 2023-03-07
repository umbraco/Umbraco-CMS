import { manifests as collectionManifests } from './collection/manifests';
import { manifests as menuItemManifests } from './sidebar-menu-item/manifests';
import { manifests as repositoryManifests } from './repository/manifests';
import { manifests as treeManifests } from './tree/manifests';
import { manifests as workspaceManifests } from './workspace/manifests';
import { manifests as entityActionManifests } from './entity-actions/manifests';
import { manifests as entityBulkActionManifests } from './entity-bulk-actions/manifests';
import { manifests as modalManifests } from './modals/manifests';

export const manifests = [
	...collectionManifests,
	...menuItemManifests,
	...treeManifests,
	...repositoryManifests,
	...workspaceManifests,
	...entityActionManifests,
	...entityBulkActionManifests,
	...modalManifests,
];
