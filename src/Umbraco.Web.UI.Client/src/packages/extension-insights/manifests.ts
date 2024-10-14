import { manifests as menuItemManifests } from './menu-item/manifests.js';
import { manifests as workspaceManifests } from './workspace/manifests.js';
import { manifests as collectionManifests } from './collection/manifests.js';
import { manifests as entityActionManifests } from './entity-actions/manifests.js';

export const manifests: Array<UmbExtensionManifest> = [
	...menuItemManifests,
	...workspaceManifests,
	...collectionManifests,
	...entityActionManifests,
];
