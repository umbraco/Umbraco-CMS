import { manifests as collectionManifests } from './collection/manifests';
import { manifests as sidebarMenuItemManifests } from './sidebar-menu-item/manifests';
import { manifests as repositoryManifests } from './repository/manifests';
import { manifests as treeManifests } from './tree/manifests';
import { manifests as workspaceManifests } from './workspace/manifests';
import { manifests as entityActionManifests } from './entity-actions/manifests';

export const manifests = [
	...collectionManifests,
	...sidebarMenuItemManifests,
	...treeManifests,
	...repositoryManifests,
	...workspaceManifests,
	...entityActionManifests,
];
