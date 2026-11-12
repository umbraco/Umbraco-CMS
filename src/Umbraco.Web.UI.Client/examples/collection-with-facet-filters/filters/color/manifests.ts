import { EXAMPLE_PRODUCT_COLOR_FILTER_ALIAS } from './constants.js';
import { EXAMPLE_PRODUCT_COLLECTION_ALIAS } from '../../collection/constants.js';
import { UMB_COLLECTION_ALIAS_CONDITION } from '@umbraco-cms/backoffice/collection';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'filter',
		alias: EXAMPLE_PRODUCT_COLOR_FILTER_ALIAS,
		name: 'Product Color Collection Filter',
		weight: 80,
		element: () => import('./color-filter.element.js'),
		api: () => import('./color-filter.api.js'),
		meta: {
			label: 'Color',
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
		alias: 'Example.FilterActiveValue.ProductCollection.Color',
		name: 'Product Color Collection Filter Active Value',
		forFilters: [EXAMPLE_PRODUCT_COLOR_FILTER_ALIAS],
		element: () => import('./color-filter-active-value.element.js'),
		valueResolver: () => import('./color-filter-value.resolver.js'),
	},
];
