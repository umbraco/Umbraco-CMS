import { UMB_MEMBER_COLLECTION_MEMBER_TYPE_FILTER_ALIAS } from './constants.js';
import { UmbMemberTypeDatalistDataSource } from './member-type-datalist-data-source.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'collectionFilter',
		kind: 'select',
		alias: UMB_MEMBER_COLLECTION_MEMBER_TYPE_FILTER_ALIAS,
		name: 'Member Type Collection Filter',
		forCollections: ['Umb.Collection.Member'],
		meta: {
			label: 'Member Type',
			datalistDataSource: UmbMemberTypeDatalistDataSource,
		},
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
