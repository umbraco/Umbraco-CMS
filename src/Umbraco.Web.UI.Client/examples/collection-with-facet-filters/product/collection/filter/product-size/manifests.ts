import { EXAMPLE_PRODUCT_COLLECTION_PRODUCT_SIZE_FILTER_ALIAS } from './constants.js';
import { EXAMPLE_PRODUCT_SIZE_ITEM_REPOSITORY_ALIAS } from '../../../../product-size/repository/item/constants.js';
import { EXAMPLE_PRODUCT_COLLECTION_ALIAS } from '../../constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'collectionFilter',
		kind: 'facetSelect',
		alias: EXAMPLE_PRODUCT_COLLECTION_PRODUCT_SIZE_FILTER_ALIAS,
		name: 'Product Size Collection Filter',
		forCollections: [EXAMPLE_PRODUCT_COLLECTION_ALIAS],
		weight: 70,
		meta: {
			label: 'Size',
			multiple: true,
			itemRepositoryAlias: EXAMPLE_PRODUCT_SIZE_ITEM_REPOSITORY_ALIAS,
		},
	},
	{
		type: 'filterActiveValue',
		kind: 'default',
		alias: 'Example.FilterActiveValue.ProductCollection.ProductSize',
		name: 'Product Size Collection Filter Active Value',
		forFilters: [EXAMPLE_PRODUCT_COLLECTION_PRODUCT_SIZE_FILTER_ALIAS],
		valueResolver: () => import('./product-size-filter-value.resolver.js'),
	},
];
