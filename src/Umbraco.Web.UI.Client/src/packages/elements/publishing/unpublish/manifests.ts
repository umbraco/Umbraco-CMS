import { manifests as entityActionManifests } from './entity-action/manifests.js';
import { manifests as entityBulkActionManifests } from './entity-bulk-action/manifests.js';
import { manifests as modalManifests } from './modal/manifests.js';
import { manifests as workspaceActionManifests } from './workspace-action/manifests.js';

export const manifests: Array<UmbExtensionManifest> = [
	...entityActionManifests,
	...entityBulkActionManifests,
	...modalManifests,
	...workspaceActionManifests,
];
