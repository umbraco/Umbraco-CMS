import { manifests as workspaceActionManifests } from './workspace-action/manifests.js';
import { manifests as workspaceActionMenuItemManifests } from './workspace-action-menu-item/manifests.js';
import { manifests as workspaceBreadcrumbManifests } from './workspace-breadcrumb/manifests.js';
import { manifests as workspaceViewManifests } from './workspace-collection/manifests.js';
import type { ManifestTypes, UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<ManifestTypes | UmbExtensionManifestKind> = [
	...workspaceActionManifests,
	...workspaceActionMenuItemManifests,
	...workspaceBreadcrumbManifests,
	...workspaceViewManifests,
];
