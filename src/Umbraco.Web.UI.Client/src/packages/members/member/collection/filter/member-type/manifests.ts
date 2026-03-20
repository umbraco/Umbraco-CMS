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
];
