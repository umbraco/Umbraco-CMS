import { manifest as imageCropsConfiguration } from './image-crops-configuration/manifests.js';
import { manifest as mediaEntityPicker } from './media-entity-picker/manifests.js';
import { manifests as imageCropperManifests } from './image-cropper/manifests.js';
import { manifests as mediaPickerManifests } from './media-picker/manifests.js';
import { manifests as uploadFieldManifests } from './upload-field/manifests.js';
import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<ManifestTypes> = [
	...imageCropperManifests,
	...mediaPickerManifests,
	...uploadFieldManifests,
	imageCropsConfiguration,
	mediaEntityPicker,
];
