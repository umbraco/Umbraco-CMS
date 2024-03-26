import { manifests as workspaceActionManifests } from './workspace-action/manifests.js';
import { manifests as workspaceActionMenuItemManifests } from './workspace-action-menu-item/manifests.js';
import { manifests as workspaceBreadcrumbManifests } from './workspace-breadcrumb/manifests.js';

export const manifests = [
	...workspaceActionManifests,
	...workspaceActionMenuItemManifests,
	...workspaceBreadcrumbManifests,
];
