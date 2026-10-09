import { manifests as documentPickerModalManifests } from './document-picker-modal/manifests.js';
import { manifest as saveModalManifest } from './save-modal/manifest.js';

export const manifests: Array<UmbExtensionManifest> = [saveModalManifest, ...documentPickerModalManifests];
