import { manifest as staticFilePickerManifest } from './static-file-picker/manifests.js';
import { manifest as staticImageFilePickerManifest } from './static-image-file-picker/manifests.js';

export const manifests: Array<UmbExtensionManifest> = [staticFilePickerManifest, staticImageFilePickerManifest];
