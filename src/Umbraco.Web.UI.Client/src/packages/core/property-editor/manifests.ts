import { manifests as uiPickerModalManifests } from './ui-picker-modal/manifests.js';
import { manifests as dataSourceManifests } from './data-source/manifests.js';

export const manifests: Array<UmbExtensionManifest> = [...uiPickerModalManifests, ...dataSourceManifests];
