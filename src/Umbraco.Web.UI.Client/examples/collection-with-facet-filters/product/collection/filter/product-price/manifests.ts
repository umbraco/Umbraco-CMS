import { EXAMPLE_PRODUCT_COLLECTION_PRODUCT_PRICE_FILTER_ALIAS } from './constants.js';
import { EXAMPLE_PRODUCT_COLLECTION_ALIAS } from '../../constants.js';
import { UMB_COLLECTION_ALIAS_CONDITION } from '@umbraco-cms/backoffice/collection';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'filter',
		alias: EXAMPLE_PRODUCT_COLLECTION_PRODUCT_PRICE_FILTER_ALIAS,
		name: 'Product Price Collection Filter',
		element: () => import('./product-price-filter.element.js'),
		api: () => import('./product-price-filter.api.js'),
		weight: 90,
		meta: {
			label: 'Price Range',
		},
		conditions: [
			{
				alias: UMB_COLLECTION_ALIAS_CONDITION,
				match: EXAMPLE_PRODUCT_COLLECTION_ALIAS,
			},
		],
	},
	{
		type: 'filterActiveValue',
		kind: 'default',
		alias: 'Example.FilterActiveValue.ProductCollection.ProductPrice',
		name: 'Product Price Collection Filter Active Value',
		forFilters: [EXAMPLE_PRODUCT_COLLECTION_PRODUCT_PRICE_FILTER_ALIAS],
		valueResolver: () => import('./product-price-filter-value.resolver.js'),
	},
];
