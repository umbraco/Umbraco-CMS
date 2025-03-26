import { manifests as dropzoneMediaTypePickerManifests } from './dropzone-media-type-picker/manifests.js';
import { manifests as imageCropperEditorManifests } from './image-cropper-editor/manifests.js';
import { manifests as mediaCaptionAltTextManifests } from './media-caption-alt-text/manifests.js';
import { manifests as mediaPickerManifests } from './media-picker/manifests.js';

export const manifests = [
	...dropzoneMediaTypePickerManifests,
	...imageCropperEditorManifests,
	...mediaCaptionAltTextManifests,
	...mediaPickerManifests,
];
