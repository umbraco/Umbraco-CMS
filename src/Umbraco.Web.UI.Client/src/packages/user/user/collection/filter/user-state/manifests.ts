import { UMB_USER_COLLECTION_ALIAS } from '../../constants.js';
import { UmbUserStateDatalistDataSource } from './user-state-datalist-data-source.js';
import { UMB_COLLECTION_ALIAS_CONDITION } from '@umbraco-cms/backoffice/collection';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'collectionFacetFilter',
		kind: 'select',
		alias: 'Umb.CollectionFacetFilter.UserState',
		name: 'User State Collection Filter',
		weight: 200,
		meta: {
			label: 'Status',
			multiple: true,
			datalistDataSource: UmbUserStateDatalistDataSource,
		},
		conditions: [
			{
				alias: UMB_COLLECTION_ALIAS_CONDITION,
				match: UMB_USER_COLLECTION_ALIAS,
			},
		],
	},
];
