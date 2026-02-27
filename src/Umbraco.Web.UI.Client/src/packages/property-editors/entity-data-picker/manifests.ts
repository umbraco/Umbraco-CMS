import { manifests as conditionManifests } from './conditions/manifests.js';
import { manifests as pickerCollectionMenuManifests } from './picker-collection/manifests.js';
import { manifests as pickerItemManifests } from './picker-item/manifests.js';
import { manifests as pickerSearchManifests } from './picker-search/manifests.js';
import { manifests as pickerTreeManifests } from './picker-tree/manifests.js';

export const manifests: Array<UmbExtensionManifest> = [
	...conditionManifests,
	...pickerCollectionMenuManifests,
	...pickerItemManifests,
	...pickerSearchManifests,
	...pickerTreeManifests,
];
