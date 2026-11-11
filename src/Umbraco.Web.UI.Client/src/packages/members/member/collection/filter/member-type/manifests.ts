import { UMB_MEMBER_COLLECTION_MEMBER_TYPE_FACET_FILTER_ALIAS } from './constants.js';
import { UmbMemberTypeDatalistDataSource } from './member-type-datalist-data-source.js';
import { UMB_COLLECTION_ALIAS_CONDITION } from '@umbraco-cms/backoffice/collection';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'facetFilter',
		kind: 'select',
		alias: UMB_MEMBER_COLLECTION_MEMBER_TYPE_FACET_FILTER_ALIAS,
		name: 'Member Type Collection Filter',
		meta: {
			label: 'Member Type',
			datalistDataSource: UmbMemberTypeDatalistDataSource,
		},
		conditions: [
			{
				alias: UMB_COLLECTION_ALIAS_CONDITION,
				match: 'Umb.Collection.Member',
			},
		],
	},
	{
		type: 'facetFilterActiveValue',
		kind: 'default',
		alias: 'Umb.FacetFilterActiveValue.MemberCollection.MemberType',
		name: 'Member Type Collection Filter Active Value',
		forFacetFilters: [UMB_MEMBER_COLLECTION_MEMBER_TYPE_FACET_FILTER_ALIAS],
		valueResolver: () => import('./member-type-facet-filter-value.resolver.js'),
	},
];
