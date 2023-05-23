import { manifests as entityActions } from './entity-actions/manifests';
import { manifests as repositoryManifests } from './repository/manifests';
import { manifests as menuItemManifests } from './menu-item/manifests';
import { manifests as treeManifests } from './tree/manifests';
import { manifests as workspaceManifests } from './workspace/manifests';
import { manifests as modalManifests } from './modals/manifests';

export const manifests = [
	...entityActions,
	...repositoryManifests,
	...menuItemManifests,
	...treeManifests,
	...workspaceManifests,
	...modalManifests,
];
