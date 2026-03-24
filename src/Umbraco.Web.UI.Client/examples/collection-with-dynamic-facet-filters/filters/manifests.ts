import { EXAMPLE_DYNAMIC_FACET_COLLECTION_ALIAS } from '../collection/constants.js';
import { UMB_COLLECTION_ALIAS_CONDITION } from '@umbraco-cms/backoffice/collection';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'collectionTextFilter',
		kind: 'default',
		alias: 'Example.DynamicFacetFilter.TextFilter',
		name: 'Dynamic Facet Filter Text Filter',
		conditions: [
			{
				alias: UMB_COLLECTION_ALIAS_CONDITION,
				match: EXAMPLE_DYNAMIC_FACET_COLLECTION_ALIAS,
			},
		],
	},
	{
		type: 'facetFilter',
		kind: 'select',
		alias: 'Example.DynamicFacetFilter.CategoryFilter',
		name: 'Dynamic Facet Category Filter',
		weight: 100,
		meta: {
			label: 'Category',
		},
		conditions: [
			{
				alias: UMB_COLLECTION_ALIAS_CONDITION,
				match: EXAMPLE_DYNAMIC_FACET_COLLECTION_ALIAS,
			},
		],
	},
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
	{
		type: 'facetFilter',
		kind: 'select',
		alias: 'Example.DynamicFacetFilter.ColorFilter',
		name: 'Dynamic Facet Color Filter',
		weight: 80,
		meta: {
			label: 'Color',
			multiple: true,
		},
		conditions: [
			{
				alias: UMB_COLLECTION_ALIAS_CONDITION,
				match: EXAMPLE_DYNAMIC_FACET_COLLECTION_ALIAS,
			},
		],
	},
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
