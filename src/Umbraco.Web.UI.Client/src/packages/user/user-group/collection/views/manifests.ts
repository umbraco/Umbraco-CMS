import { UMB_USER_GROUP_TABLE_COLLECTION_VIEW_ALIAS } from './constants.js';
import { UMB_COLLECTION_ALIAS_CONDITION } from '@umbraco-cms/backoffice/collection';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'collectionView',
		alias: UMB_USER_GROUP_TABLE_COLLECTION_VIEW_ALIAS,
		name: 'User Group Table Collection View',
		js: () => import('./user-group-table-collection-view.element.js'),
		meta: {
			label: 'Table',
			icon: 'icon-list',
			pathName: 'table',
		},
		conditions: [
			{
				alias: UMB_COLLECTION_ALIAS_CONDITION,
				match: 'Umb.Collection.UserGroup',
			},
		],
	},
];
