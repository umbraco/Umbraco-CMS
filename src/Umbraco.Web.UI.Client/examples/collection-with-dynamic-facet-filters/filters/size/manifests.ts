import { EXAMPLE_DYNAMIC_FACET_COLLECTION_ALIAS } from '../../collection/constants.js';
import { UMB_COLLECTION_ALIAS_CONDITION } from '@umbraco-cms/backoffice/collection';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'facetFilter',
		kind: 'select',
		alias: 'Example.DynamicFacetFilter.SizeFilter',
		name: 'Dynamic Facet Size Filter',
		weight: 70,
		meta: {
			label: 'Size',
			multiple: true,
		},
		conditions: [
			{
				alias: UMB_COLLECTION_ALIAS_CONDITION,
				match: EXAMPLE_DYNAMIC_FACET_COLLECTION_ALIAS,
			},
		],
	},
];
