import { manifests as entityActions } from './entity-actions/manifests.js';
import { manifests as repositoryManifests } from './repository/manifests.js';
import { manifests as menuManifests } from './menu/manifests.js';
import { manifests as treeManifests } from './tree/manifests.js';
import { manifests as workspaceManifests } from './workspace/manifests.js';
import { manifests as modalManifests } from './modals/manifests.js';
import { manifests as collectionManifests } from './collection/manifests.js';

export const manifests = [
	...entityActions,
	...repositoryManifests,
	...menuManifests,
	...treeManifests,
	...workspaceManifests,
	...modalManifests,
	...collectionManifests,
];
