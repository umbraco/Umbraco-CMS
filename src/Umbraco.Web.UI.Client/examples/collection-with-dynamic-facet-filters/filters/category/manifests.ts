import { ExampleCategoryDatalistDataSource } from './category-datalist-data-source.js';
import { EXAMPLE_PRODUCT_CATEGORY_FACET_FILTER_ALIAS } from './constants.js';
import { EXAMPLE_PRODUCT_COLLECTION_ALIAS } from '../../collection/constants.js';
import { UMB_COLLECTION_ALIAS_CONDITION } from '@umbraco-cms/backoffice/collection';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'facetFilter',
		kind: 'select',
		alias: EXAMPLE_PRODUCT_CATEGORY_FACET_FILTER_ALIAS,
		name: 'Dynamic Facet Category Filter',
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
		type: 'facetFilterActiveValue',
		kind: 'default',
		alias: 'Example.FacetFilterActiveValue.ProductCollection.Category',
		name: 'Category Active Filter Value',
		forFacetFilters: [EXAMPLE_PRODUCT_CATEGORY_FACET_FILTER_ALIAS],
		valueResolver: () => import('./category-facet-filter-value.resolver.js'),
	},
];
