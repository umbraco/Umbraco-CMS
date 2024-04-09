import { manifests as workspaceActionManifests } from './workspace-action/manifests.js';
import { manifests as workspaceActionMenuItemManifests } from './workspace-action-menu-item/manifests.js';
import { manifests as workspaceBreadcrumbManifests } from './workspace-breadcrumb/manifests.js';
import { manifests as workspaceViewManifests } from './workspace-collection/manifests.js';

export const manifests = [
	...workspaceActionManifests,
	...workspaceActionMenuItemManifests,
	...workspaceBreadcrumbManifests,
	...workspaceViewManifests,
];
