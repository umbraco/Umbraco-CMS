import { UMB_USER_COLLECTION_ALIAS } from '../../constants.js';
import { UmbUserGroupDatalistDataSource } from './user-group-datalist-data-source.js';
import { UMB_COLLECTION_ALIAS_CONDITION } from '@umbraco-cms/backoffice/collection';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'collectionFacetFilter',
		kind: 'select',
		alias: 'Umb.CollectionFacetFilter.UserGroup',
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
