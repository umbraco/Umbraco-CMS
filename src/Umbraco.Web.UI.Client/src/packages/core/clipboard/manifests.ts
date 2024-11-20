import { manifests as clipboardRootManifests } from './clipboard-root/manifests.js';
import { manifests as collectionManifests } from './collection/manifests.js';
import { manifests as contextManifests } from './context/manifests.js';
import { manifests as detailManifests } from './detail/manifests.js';
import { manifests as pickerModalManifests } from './picker-modal/manifests.js';

export const manifests = [
	...clipboardRootManifests,
	...collectionManifests,
	...contextManifests,
	...detailManifests,
	...pickerModalManifests,
];
