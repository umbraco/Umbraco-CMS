import { manifests as pickerManifests } from './picker/manifests.js';
import { manifests as removeManifests } from './remove/manifests.js';

export const manifests: Array<UmbExtensionManifest> = [...pickerManifests, ...removeManifests];
