import { UmbMemberGroupDatalistDataSource } from './member-group-datalist-data-source.js';
import { UMB_COLLECTION_ALIAS_CONDITION } from '@umbraco-cms/backoffice/collection';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'facetFilter',
		kind: 'select',
		alias: 'Umb.CollectionFacetFilter.MemberGroup',
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
];
