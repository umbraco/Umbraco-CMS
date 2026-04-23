import { UMB_USER_COLLECTION_USER_GROUP_FACET_FILTER_ALIAS } from './constants.js';
import { UMB_USER_COLLECTION_ALIAS } from '../../constants.js';
import { UmbUserGroupDatalistDataSource } from './user-group-datalist-data-source.js';
import { UMB_COLLECTION_ALIAS_CONDITION } from '@umbraco-cms/backoffice/collection';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'facetFilter',
		kind: 'select',
		alias: UMB_USER_COLLECTION_USER_GROUP_FACET_FILTER_ALIAS,
		name: 'User Group Collection Filter',
		weight: 100,
		meta: {
			label: 'Groups',
			multiple: true,
			datalistDataSource: UmbUserGroupDatalistDataSource,
		},
		conditions: [
			{
				alias: UMB_COLLECTION_ALIAS_CONDITION,
				match: UMB_USER_COLLECTION_ALIAS,
			},
		],
	},
];
