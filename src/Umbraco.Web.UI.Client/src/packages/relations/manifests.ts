import { manifests as menuManifests } from './menu/manifests.js';
import { manifests as relationManifests } from './relations/manifests.js';
import { manifests as relationTypeManifests } from './relation-types/manifests.js';
import { manifests as workspaceManifests } from './workspace/manifests.js';

export const manifests: Array<UmbExtensionManifest> = [
	...menuManifests,
	...relationManifests,
	...relationTypeManifests,
	...workspaceManifests,
];
