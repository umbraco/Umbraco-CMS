import { EXAMPLE_PRODUCT_COLLECTION_ALIAS } from '../../collection/constants.js';
import { ExampleCategoryDatalistDataSource } from './category-datalist-data-source.js';
import { EXAMPLE_PRODUCT_CATEGORY_FILTER_ALIAS } from './constants.js';
import { UMB_COLLECTION_ALIAS_CONDITION } from '@umbraco-cms/backoffice/collection';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'filter',
		kind: 'select',
		alias: EXAMPLE_PRODUCT_CATEGORY_FILTER_ALIAS,
		name: 'Product Category Collection Filter',
		weight: 100,
		meta: {
			label: 'Category',
			datalistDataSource: ExampleCategoryDatalistDataSource,
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
		alias: 'Example.FilterActiveValue.ProductCollection.Category',
		name: 'Product Category Collection Filter Active Value',
		forFilters: [EXAMPLE_PRODUCT_CATEGORY_FILTER_ALIAS],
		valueResolver: () => import('./category-filter-value.resolver.js'),
	},
];
