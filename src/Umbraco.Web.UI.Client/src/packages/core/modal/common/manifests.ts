import { manifests as confirmManifests } from './confirm/manifests.js';
import { manifests as discardChangesManifests } from './discard-changes/manifests.js';
import { manifests as itemPickerManifests } from './item-picker/manifests.js';

export const manifests: Array<UmbExtensionManifest> = [
	...confirmManifests,
	...discardChangesManifests,
	...itemPickerManifests,
];
