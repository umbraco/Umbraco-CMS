import { manifests as memberPickerManifests } from './member-picker/manifests.js';
import { manifests as multipleMemberPickerManifests } from './multiple-member-picker/manifests.js';

export const manifests: Array<UmbExtensionManifest> = [...memberPickerManifests, ...multipleMemberPickerManifests];
