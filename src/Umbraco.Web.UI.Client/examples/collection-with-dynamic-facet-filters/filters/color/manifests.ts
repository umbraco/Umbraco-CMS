import { EXAMPLE_PRODUCT_COLOR_FACET_FILTER_ALIAS } from './constants.js';
import { EXAMPLE_PRODUCT_COLLECTION_ALIAS } from '../../collection/constants.js';
import { UMB_COLLECTION_ALIAS_CONDITION } from '@umbraco-cms/backoffice/collection';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'facetFilter',
		alias: EXAMPLE_PRODUCT_COLOR_FACET_FILTER_ALIAS,
		name: 'Dynamic Facet Color Filter',
		weight: 80,
		element: () => import('./color-facet-filter.element.js'),
		api: () => import('./color-facet-filter.api.js'),
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
		type: 'facetFilterActiveValue',
		kind: 'default',
		alias: 'Example.FacetFilterActiveValue.ProductCollection.Color',
		name: 'Color Active Filter Value',
		forFacetFilters: [EXAMPLE_PRODUCT_COLOR_FACET_FILTER_ALIAS],
		element: () => import('./color-facet-filter-active-value.element.js'),
		valueResolver: () => import('./color-facet-filter-value.resolver.js'),
	},
];
