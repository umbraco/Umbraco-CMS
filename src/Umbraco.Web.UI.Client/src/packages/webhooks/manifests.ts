import { manifests as repositoryManifests } from './repository/manifests.js';
import { manifests as treeManifests } from './menu-item/manifests.js';
//import { manifests as entityActions } from './entity-actions/manifests.js';
import { manifests as workspaceManifests } from './workspace/manifests.js';
import { manifests as collectionManifests } from './collection/manifests.js';

export const manifests = [
	...repositoryManifests,
	//...entityActions,
	...treeManifests,
	...workspaceManifests,
	...collectionManifests,
];
