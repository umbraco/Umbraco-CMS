import { manifests as previewOptionManifests } from './workspace-action-menu-item/manifests.js';
import { manifests as workspaceActionManifests } from './workspace-action/manifests.js';
import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [
	...previewOptionManifests,
	...workspaceActionManifests,
];
