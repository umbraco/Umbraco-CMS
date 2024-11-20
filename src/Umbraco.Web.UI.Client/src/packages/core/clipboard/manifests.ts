import { manifests as collectionManifests } from './collection/manifests.js';
import { manifests as contextManifests } from './context/manifests.js';
import { manifests as entryManifests } from './clipboard-entry/manifests.js';
import { manifests as rootManifests } from './clipboard-root/manifests.js';

export const manifests: Array<UmbExtensionManifest> = [
	...collectionManifests,
	...contextManifests,
	...entryManifests,
	...rootManifests,
];
