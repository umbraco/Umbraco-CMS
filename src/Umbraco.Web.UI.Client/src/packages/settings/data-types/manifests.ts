import { manifests as entityActions } from './entity-actions/manifests.js';
import { manifests as repositoryManifests } from './repository/manifests.js';
import { manifests as menuItemManifests } from './menu-item/manifests.js';
import { manifests as treeManifests } from './tree/manifests.js';
import { manifests as workspaceManifests } from './workspace/manifests.js';
import { manifests as modalManifests } from './modals/manifests.js';

export const manifests = [
	...entityActions,
	...repositoryManifests,
	...menuItemManifests,
	...treeManifests,
	...workspaceManifests,
	...modalManifests,
];
