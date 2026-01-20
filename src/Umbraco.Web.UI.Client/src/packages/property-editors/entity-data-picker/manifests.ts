import { pickerViewsConfigurationManifest } from './property-editor/config/index.js';
import { manifests as pickerCollectionMenuManifests } from './picker-collection/manifests.js';
import { manifests as pickerItemManifests } from './picker-item/manifests.js';
import { manifests as pickerSearchManifests } from './picker-search/manifests.js';
import { manifests as pickerTreeManifests } from './picker-tree/manifests.js';
import { manifests as propertyEditorManifests } from './property-editor/manifests.js';

export const manifests: Array<UmbExtensionManifest> = [
	pickerViewsConfigurationManifest,
	...pickerCollectionMenuManifests,
	...pickerItemManifests,
	...pickerSearchManifests,
	...pickerTreeManifests,
	...propertyEditorManifests,
];
