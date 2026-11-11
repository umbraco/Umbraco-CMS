import { EXAMPLE_PRODUCT_PRICE_FACET_FILTER_ALIAS } from './constants.js';
import { EXAMPLE_PRODUCT_COLLECTION_ALIAS } from '../../collection/constants.js';
import { UMB_COLLECTION_ALIAS_CONDITION } from '@umbraco-cms/backoffice/collection';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'facetFilter',
		alias: EXAMPLE_PRODUCT_PRICE_FACET_FILTER_ALIAS,
		name: 'Dynamic Facet Price Filter',
		kind: 'range',
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
		type: 'facetFilterActiveValue',
		kind: 'default',
		alias: 'Example.FacetFilterActiveValue.ProductCollection.Price',
		name: 'Price Range Active Filter Value',
		forFacetFilters: [EXAMPLE_PRODUCT_PRICE_FACET_FILTER_ALIAS],
		valueResolver: () => import('../../range-kind/range-facet-filter-value.resolver.js'),
	},
];
