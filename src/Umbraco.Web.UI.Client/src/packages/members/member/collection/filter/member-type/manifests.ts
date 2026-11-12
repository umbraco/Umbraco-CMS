import { UMB_MEMBER_COLLECTION_MEMBER_TYPE_FILTER_ALIAS } from './constants.js';
import { UmbMemberTypeDatalistDataSource } from './member-type-datalist-data-source.js';
import { UMB_COLLECTION_ALIAS_CONDITION } from '@umbraco-cms/backoffice/collection';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'filter',
		kind: 'select',
		alias: UMB_MEMBER_COLLECTION_MEMBER_TYPE_FILTER_ALIAS,
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
		type: 'filterActiveValue',
		kind: 'default',
		alias: 'Umb.FilterActiveValue.MemberCollection.MemberType',
		name: 'Member Type Collection Filter Active Value',
		forFilters: [UMB_MEMBER_COLLECTION_MEMBER_TYPE_FILTER_ALIAS],
		valueResolver: () => import('./member-type-filter-value.resolver.js'),
	},
];
