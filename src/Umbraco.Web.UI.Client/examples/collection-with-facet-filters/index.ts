import { manifests as productCategoryManifests } from './product-category/manifests.js';
import { manifests as productColorManifests } from './product-color/manifests.js';
import { manifests as dashboardManifests } from './dashboard/manifests.js';
import { manifests as productManifests } from './product/manifests.js';
import { manifests as productSizeManifests } from './product-size/manifests.js';

export const manifests: Array<UmbExtensionManifest> = [
	...productCategoryManifests,
	...productColorManifests,
	...dashboardManifests,
	...productManifests,
	...productSizeManifests,
];
