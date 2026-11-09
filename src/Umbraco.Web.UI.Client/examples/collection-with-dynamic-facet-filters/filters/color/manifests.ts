import { EXAMPLE_PRODUCT_COLLECTION_ALIAS } from '../../collection/constants.js';
import { UMB_COLLECTION_ALIAS_CONDITION } from '@umbraco-cms/backoffice/collection';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'facetFilter',
		alias: 'Example.DynamicFacetFilter.ColorFilter',
		name: 'Dynamic Facet Color Filter',
		weight: 80,
		element: () => import('./color-facet-filter.element.js'),
		api: () => import('./color-facet-filter.api.js'),
		meta: {
			label: 'Color',
		},
		valueResolver: () => import('./color-facet-filter-value.resolver.js'),
		conditions: [
			{
				alias: UMB_COLLECTION_ALIAS_CONDITION,
				match: EXAMPLE_PRODUCT_COLLECTION_ALIAS,
			},
		],
	},
];
