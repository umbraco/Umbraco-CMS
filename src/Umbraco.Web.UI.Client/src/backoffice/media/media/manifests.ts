import { manifests as repositoryManifests } from './repository/manifests';
import { manifests as menuItemManifests } from './menu-item/manifests';
import { manifests as treeManifests } from './tree/manifests';
import { manifests as workspaceManifests } from './workspace/manifests';
import { manifests as entityActionsManifests } from './entity-actions/manifests';
import { manifests as entityBulkActionsManifests } from './entity-bulk-actions/manifests';
import { manifests as modalManifests } from './modals/manifests';

export const manifests = [
	...repositoryManifests,
	...menuItemManifests,
	...treeManifests,
	...workspaceManifests,
	...entityActionsManifests,
	...entityBulkActionsManifests,
	...modalManifests,
];
