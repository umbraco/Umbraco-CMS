import { manifests as uiPickerModalManifests } from './ui-picker-modal/manifests.js';
import { manifests as propertyEditorDataSourceManifests } from './property-editor-data-source/manifests.js';

export const manifests: Array<UmbExtensionManifest> = [...uiPickerModalManifests, ...propertyEditorDataSourceManifests];
