import { manifests as repositoryManifests } from './repository/manifests';
import { manifests as sidebarMenuItemManifests } from './sidebar-menu-item/manifests';
import { manifests as treeManifests } from './tree/manifests';
import { manifests as workspaceManifests } from './workspace/manifests';
import { manifests as entityActionsManifests } from './entity-actions/manifests';

export const manifests = [
	...repositoryManifests,
	...sidebarMenuItemManifests,
	...treeManifests,
	...workspaceManifests,
	...entityActionsManifests,
];
