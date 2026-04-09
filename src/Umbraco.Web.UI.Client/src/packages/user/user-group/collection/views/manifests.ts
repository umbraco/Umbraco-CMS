import { UMB_SECTION_ALIASES_VALUE_TYPE } from '../../value-summary/constants.js';
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
			icon: 'icon-table',
			pathName: 'table',
		},
		conditions: [
			{
				alias: UMB_COLLECTION_ALIAS_CONDITION,
				match: 'Umb.Collection.UserGroup',
			},
		],
	},
	{
		type: 'collectionView',
		kind: 'table',
		alias: 'Umb.CollectionView.UserGroup.Table.Kind',
		name: 'User Group Table Kind Collection View',
		meta: {
			label: 'Table',
			icon: 'icon-table',
			pathName: 'table2',
			columns: [
				{
					field: 'sections',
					label: '#main_sections',
					valueType: UMB_SECTION_ALIASES_VALUE_TYPE,
				},
				{
					field: 'documentStartNode',
					label: '#user_startnode',
					// TODO: Use UMB_DOCUMENT_START_NODE_VALUE_TYPE when documents have been decoupled from users.
					valueType: 'Umb.ValueType.Document.StartNode',
				},
				{
					field: 'mediaStartNode',
					label: '#user_mediastartnode',
					// TODO: Use UMB_MEDIA_START_NODE_VALUE_TYPE when media have been decoupled from users.
					valueType: 'Umb.ValueType.Media.StartNode',
				},
			],
		},
		conditions: [
			{
				alias: UMB_COLLECTION_ALIAS_CONDITION,
				match: 'Umb.Collection.UserGroup',
			},
		],
	},
];
