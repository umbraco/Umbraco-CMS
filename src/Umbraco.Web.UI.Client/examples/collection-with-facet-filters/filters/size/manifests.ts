import { ExampleSizeDatalistDataSource } from './size-datalist-data-source.js';
import { EXAMPLE_PRODUCT_SIZE_FILTER_ALIAS } from './constants.js';
import { EXAMPLE_PRODUCT_COLLECTION_ALIAS } from '../../collection/constants.js';
import { UMB_COLLECTION_ALIAS_CONDITION } from '@umbraco-cms/backoffice/collection';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'filter',
		kind: 'select',
		alias: EXAMPLE_PRODUCT_SIZE_FILTER_ALIAS,
		name: 'Product Size Collection Filter',
		weight: 70,
		meta: {
			label: 'Size',
			multiple: true,
			datalistDataSource: ExampleSizeDatalistDataSource,
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
		alias: 'Example.FilterActiveValue.ProductCollection.Size',
		name: 'Product Size Collection Filter Active Value',
		forFilters: [EXAMPLE_PRODUCT_SIZE_FILTER_ALIAS],
		valueResolver: () => import('./size-filter-value.resolver.js'),
	},
];
