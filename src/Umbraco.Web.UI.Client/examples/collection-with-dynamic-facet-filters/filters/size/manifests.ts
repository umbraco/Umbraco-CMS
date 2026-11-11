import { ExampleSizeDatalistDataSource } from './size-datalist-data-source.js';
import { EXAMPLE_PRODUCT_SIZE_FACET_FILTER_ALIAS } from './constants.js';
import { EXAMPLE_PRODUCT_COLLECTION_ALIAS } from '../../collection/constants.js';
import { UMB_COLLECTION_ALIAS_CONDITION } from '@umbraco-cms/backoffice/collection';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'facetFilter',
		kind: 'select',
		alias: EXAMPLE_PRODUCT_SIZE_FACET_FILTER_ALIAS,
		name: 'Dynamic Facet Size Filter',
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
		type: 'facetFilterActiveValue',
		kind: 'default',
		alias: 'Example.FacetFilterActiveValue.ProductCollection.Size',
		name: 'Size Active Filter Value',
		forFacetFilters: [EXAMPLE_PRODUCT_SIZE_FACET_FILTER_ALIAS],
		valueResolver: () => import('./size-facet-filter-value.resolver.js'),
	},
];
