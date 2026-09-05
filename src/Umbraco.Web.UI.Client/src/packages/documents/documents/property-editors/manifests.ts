import { manifests as documentPickerManifests } from './document-picker/manifests.js';
import { manifests as multipleDocumentPickerManifests } from './multiple-document-picker/manifests.js';

export const manifests: Array<UmbExtensionManifest> = [...documentPickerManifests, ...multipleDocumentPickerManifests];
