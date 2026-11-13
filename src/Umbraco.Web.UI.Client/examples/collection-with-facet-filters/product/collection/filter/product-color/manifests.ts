import { EXAMPLE_PRODUCT_COLLECTION_PRODUCT_COLOR_FILTER_ALIAS } from './constants.js';
import { EXAMPLE_PRODUCT_COLOR_ITEM_REPOSITORY_ALIAS } from '../../../../product-color/repository/item/constants.js';
import { EXAMPLE_PRODUCT_COLLECTION_ALIAS } from '../../constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'collectionFilter',
		kind: 'facetSelect',
		alias: EXAMPLE_PRODUCT_COLLECTION_PRODUCT_COLOR_FILTER_ALIAS,
		name: 'Product Color Collection Filter',
		forCollections: [EXAMPLE_PRODUCT_COLLECTION_ALIAS],
		weight: 80,
		element: () => import('./product-color-filter.element.js'),
		api: () => import('./product-color-filter.api.js'),
		meta: {
			label: 'Color',
			itemRepositoryAlias: EXAMPLE_PRODUCT_COLOR_ITEM_REPOSITORY_ALIAS,
		},
	},
	{
		type: 'filterActiveValue',
		kind: 'default',
		alias: 'Example.FilterActiveValue.ProductCollection.ProductColor',
		name: 'Product Color Collection Filter Active Value',
		forFilters: [EXAMPLE_PRODUCT_COLLECTION_PRODUCT_COLOR_FILTER_ALIAS],
		element: () => import('./product-color-filter-active-value.element.js'),
		valueResolver: () => import('./product-color-filter-value.resolver.js'),
	},
];
