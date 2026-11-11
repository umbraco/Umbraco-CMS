import { UMB_MEMBER_COLLECTION_MEMBER_GROUP_FACET_FILTER_ALIAS } from './constants.js';
import { UmbMemberGroupDatalistDataSource } from './member-group-datalist-data-source.js';
import { UMB_COLLECTION_ALIAS_CONDITION } from '@umbraco-cms/backoffice/collection';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'facetFilter',
		kind: 'select',
		alias: UMB_MEMBER_COLLECTION_MEMBER_GROUP_FACET_FILTER_ALIAS,
		name: 'Member Group Collection Filter',
		meta: {
			label: 'Member Group',
			datalistDataSource: UmbMemberGroupDatalistDataSource,
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
		alias: 'Umb.FacetFilterActiveValue.MemberCollection.MemberGroup',
		name: 'Member Group Collection Filter Active Value',
		forFacetFilters: [UMB_MEMBER_COLLECTION_MEMBER_GROUP_FACET_FILTER_ALIAS],
		valueResolver: () => import('./member-group-facet-filter-value.resolver.js'),
	},
];
