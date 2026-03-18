import { UMB_COLLECTION_ALIAS_CONDITION } from '@umbraco-cms/backoffice/collection';
import { UmbMemberGroupDatalistDataSource } from './member-group-datalist-data-source.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'collectionFacetFilter',
		kind: 'select',
		alias: 'Umb.CollectionFacetFilter.MemberGroup',
		name: 'Member Group Collection Filter',
		meta: {
			label: 'Member Group',
			filterKey: 'memberGroupName',
			datalistDataSource: UmbMemberGroupDatalistDataSource,
		},
		conditions: [
			{
				alias: UMB_COLLECTION_ALIAS_CONDITION,
				match: 'Umb.Collection.Member',
			},
		],
	},
];
