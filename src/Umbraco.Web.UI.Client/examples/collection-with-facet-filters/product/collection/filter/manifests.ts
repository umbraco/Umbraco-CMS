import { EXAMPLE_PRODUCT_COLLECTION_ALIAS } from '../constants.js';
import { manifests as productCategoryManifests } from './product-category/manifests.js';
import { manifests as productColorManifests } from './product-color/manifests.js';
import { manifests as productPriceManifests } from './product-price/manifests.js';
import { manifests as productSizeManifests } from './product-size/manifests.js';
import { UMB_COLLECTION_ALIAS_CONDITION } from '@umbraco-cms/backoffice/collection';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'collectionTextFilter',
		kind: 'default',
		alias: 'Example.CollectionTextFilter.Product',
		name: 'Product Collection Text Filter',
		conditions: [
			{
				alias: UMB_COLLECTION_ALIAS_CONDITION,
				match: EXAMPLE_PRODUCT_COLLECTION_ALIAS,
			},
		],
	},
	...productCategoryManifests,
	...productColorManifests,
	...productPriceManifests,
	...productSizeManifests,
];
