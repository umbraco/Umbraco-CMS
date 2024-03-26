import { manifests as repositoryManifests } from './repository/manifests.js';
import { manifests as menuManifests } from './menu/manifests.js';
import { manifests as treeManifests } from './tree/manifests.js';
import { manifests as workspaceManifests } from './workspace/manifests.js';
import { manifests as entityActionManifests } from './entity-actions/manifests.js';

export const manifests = [
	...repositoryManifests,
	...menuManifests,
	...treeManifests,
	...workspaceManifests,
	...entityActionManifests,
];
