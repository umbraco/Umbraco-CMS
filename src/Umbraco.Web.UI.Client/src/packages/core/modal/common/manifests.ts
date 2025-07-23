import { manifests as confirmManifests } from './confirm/manifests.js';
import { manifests as discardChangesManifests } from './discard-changes/manifests.js';
import { manifests as errorViewerManifests } from './error-viewer/manifest.js';
import { manifests as itemPickerManifests } from './item-picker/manifests.js';

export const manifests: Array<UmbExtensionManifest> = [
	...confirmManifests,
	...discardChangesManifests,
	...errorViewerManifests,
	...itemPickerManifests,
];
