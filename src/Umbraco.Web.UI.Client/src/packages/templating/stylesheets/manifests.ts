import { manifests as repositoryManifests } from './repository/manifests.js';
import { manifests as menuItemManifests } from './menu-item/manifests.js';
import { manifests as treeManifests } from './tree/manifests.js';
import { manifests as workspaceManifests } from './workspace/manifests.js';
import { manifests as entityActionManifests } from './entity-actions/manifests.js';
import { manifests as componentManifests } from './components/manifests.js';

export const manifests = [
	...repositoryManifests,
	...menuItemManifests,
	...treeManifests,
	...workspaceManifests,
	...entityActionManifests,
	...componentManifests,
];
