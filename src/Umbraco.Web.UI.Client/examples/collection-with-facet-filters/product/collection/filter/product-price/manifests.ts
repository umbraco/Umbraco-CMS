import { EXAMPLE_PRODUCT_COLLECTION_PRODUCT_PRICE_FILTER_ALIAS } from './constants.js';
import { EXAMPLE_PRODUCT_COLLECTION_ALIAS } from '../../constants.js';
import { manifests as localizationManifests } from './localization/manifests.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'collectionFilter',
		alias: EXAMPLE_PRODUCT_COLLECTION_PRODUCT_PRICE_FILTER_ALIAS,
		name: 'Product Price Collection Filter',
		forCollections: [EXAMPLE_PRODUCT_COLLECTION_ALIAS],
		element: () => import('./product-price-filter.element.js'),
		api: () => import('./product-price-filter.api.js'),
		weight: 90,
		meta: {
			label: 'Price',
		},
	},
	{
		type: 'filterActiveValue',
		kind: 'default',
		alias: 'Example.FilterActiveValue.ProductCollection.ProductPrice',
		name: 'Product Price Collection Filter Active Value',
		forFilters: [EXAMPLE_PRODUCT_COLLECTION_PRODUCT_PRICE_FILTER_ALIAS],
		element: () => import('./product-price-filter-active-value.element.js'),
	},
	...localizationManifests,
];
