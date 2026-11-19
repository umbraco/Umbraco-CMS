import { manifest as imageCropsConfiguration } from './image-crops/manifests.js';
import { manifest as mediaEntityPicker } from './media-entity-picker/manifests.js';
import { manifests as imageCropperManifests } from './image-cropper/manifests.js';
import { manifests as mediaPickerManifests } from './media-picker/manifests.js';
import { manifests as mediaStartNodeAccessManifests } from './media-start-node-access/manifests.js';
import { manifests as uploadFieldManifests } from './upload-field/manifests.js';

export const manifests: Array<UmbExtensionManifest> = [
	...imageCropperManifests,
	...mediaPickerManifests,
	...mediaStartNodeAccessManifests,
	...uploadFieldManifests,
	imageCropsConfiguration,
	mediaEntityPicker,
];
