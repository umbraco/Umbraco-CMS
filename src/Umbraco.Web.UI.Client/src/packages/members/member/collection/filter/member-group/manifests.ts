import { UMB_MEMBER_COLLECTION_MEMBER_GROUP_FILTER_ALIAS } from './constants.js';
import { UmbMemberGroupDatalistDataSource } from './member-group-datalist-data-source.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'collectionFilter',
		kind: 'select',
		alias: UMB_MEMBER_COLLECTION_MEMBER_GROUP_FILTER_ALIAS,
		name: 'Member Group Collection Filter',
		forCollections: ['Umb.Collection.Member'],
		meta: {
			label: 'Member Group',
			datalistDataSource: UmbMemberGroupDatalistDataSource,
		},
	},
	{
		type: 'filterActiveValue',
		kind: 'default',
		alias: 'Umb.FilterActiveValue.MemberCollection.MemberGroup',
		name: 'Member Group Collection Filter Active Value',
		forFilters: [UMB_MEMBER_COLLECTION_MEMBER_GROUP_FILTER_ALIAS],
		valueResolver: () => import('./member-group-filter-value.resolver.js'),
	},
];
