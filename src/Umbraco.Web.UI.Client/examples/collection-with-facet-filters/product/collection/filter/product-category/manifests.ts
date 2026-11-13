import { EXAMPLE_PRODUCT_COLLECTION_PRODUCT_CATEGORY_FILTER_ALIAS } from './constants.js';
import { EXAMPLE_PRODUCT_CATEGORY_ITEM_REPOSITORY_ALIAS } from '../../../../product-category/repository/item/constants.js';
import { EXAMPLE_PRODUCT_COLLECTION_ALIAS } from '../../constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'collectionFilter',
		kind: 'facetSelect',
		alias: EXAMPLE_PRODUCT_COLLECTION_PRODUCT_CATEGORY_FILTER_ALIAS,
		name: 'Product Category Collection Filter',
		forCollections: [EXAMPLE_PRODUCT_COLLECTION_ALIAS],
		weight: 100,
		meta: {
			label: 'Category',
			itemRepositoryAlias: EXAMPLE_PRODUCT_CATEGORY_ITEM_REPOSITORY_ALIAS,
		},
	},
	{
		type: 'filterActiveValue',
		kind: 'default',
		alias: 'Example.FilterActiveValue.ProductCollection.ProductCategory',
		name: 'Product Category Collection Filter Active Value',
		forFilters: [EXAMPLE_PRODUCT_COLLECTION_PRODUCT_CATEGORY_FILTER_ALIAS],
		valueResolver: () => import('./product-category-filter-value.resolver.js'),
	},
];
