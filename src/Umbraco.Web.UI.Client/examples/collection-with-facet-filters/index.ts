import { manifests as productCategoryManifests } from './product-category/manifests.js';
import { manifests as productColorManifests } from './product-color/manifests.js';
import { manifests as dashboardManifests } from './dashboard/manifests.js';
import { manifests as productManifests } from './product/manifests.js';
import { manifests as rangeKindManifests } from './range-kind/manifests.js';
import { manifests as productSizeManifests } from './product-size/manifests.js';
import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [
	...productCategoryManifests,
	...productColorManifests,
	...dashboardManifests,
	...productManifests,
	...rangeKindManifests,
	...productSizeManifests,
];
