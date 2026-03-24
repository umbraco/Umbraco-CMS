import { EXAMPLE_DYNAMIC_FACET_COLLECTION_ALIAS } from '../../collection/constants.js';
import { UMB_COLLECTION_ALIAS_CONDITION } from '@umbraco-cms/backoffice/collection';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'facetFilter',
		alias: 'Example.DynamicFacetFilter.PriceFilter',
		name: 'Dynamic Facet Price Filter',
		kind: 'range',
		weight: 90,
		meta: {
			label: 'Price Range',
		},
		conditions: [
			{
				alias: UMB_COLLECTION_ALIAS_CONDITION,
				match: EXAMPLE_DYNAMIC_FACET_COLLECTION_ALIAS,
			},
		],
	},
];
