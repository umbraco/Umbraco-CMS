import { manifests as menuManifests } from './menu.manifests';
import { manifests as menuItemManifests } from './menu-item/manifests';
import { manifests as treeManifests } from './tree/manifests';
import { manifests as repositoryManifests } from './repository/manifests';
import { manifests as workspaceManifests } from './workspace/manifests';
import { manifests as entityActionManifests } from './entity-actions/manifests';

export const manifests = [
	...menuManifests,
	...menuItemManifests,
	...treeManifests,
	...repositoryManifests,
	...workspaceManifests,
	...entityActionManifests,
];
