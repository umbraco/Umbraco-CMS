import { manifests as documentPickerManifests } from './document-picker/manifests.js';
import { manifests as documentStartNodeAccessManifests } from './document-start-node-access/manifests.js';

export const manifests: Array<UmbExtensionManifest> = [...documentPickerManifests, ...documentStartNodeAccessManifests];
