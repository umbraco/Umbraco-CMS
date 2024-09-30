import { manifests as workspaceViewManifests } from '../../content/collection/manifests.js';
import { manifests as workspaceActionManifests } from './workspace-action/manifests.js';
import { manifests as workspaceActionMenuItemManifests } from './workspace-action-menu-item/manifests.js';
import { manifests as workspaceBreadcrumbManifests } from './workspace-breadcrumb/manifests.js';
import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [
	...workspaceActionManifests,
	...workspaceActionMenuItemManifests,
	...workspaceBreadcrumbManifests,
	...workspaceViewManifests,
];
