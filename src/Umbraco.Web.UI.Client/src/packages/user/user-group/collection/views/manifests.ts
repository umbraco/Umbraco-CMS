import { UMB_USER_GROUP_COLLECTION_ALIAS } from '../constants.js';
import { UMB_USER_GROUP_TABLE_COLLECTION_VIEW_ALIAS } from './constants.js';
import { UMB_SECTION_ALIASES_VALUE_TYPE } from '@umbraco-cms/backoffice/section';
import { UMB_COLLECTION_ALIAS_CONDITION } from '@umbraco-cms/backoffice/collection';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'collectionView',
		kind: 'table',
		alias: UMB_USER_GROUP_TABLE_COLLECTION_VIEW_ALIAS,
		name: 'User Group Table Collection View',
		meta: {
			columns: [
				{
					field: 'sections',
					label: '#main_sections',
					valueType: UMB_SECTION_ALIASES_VALUE_TYPE,
				},
				{
					field: 'documentStartNode',
					label: '#user_startnode',
					// TODO: Use UMB_DOCUMENT_USER_START_NODE_VALUE_TYPE when documents have been decoupled from users.
					valueType: 'Umb.ValueType.Document.UserStartNode',
				},
				{
					field: 'mediaStartNode',
					label: '#user_mediastartnode',
					// TODO: Use UMB_MEDIA_USER_START_NODE_VALUE_TYPE when media have been decoupled from users.
					valueType: 'Umb.ValueType.Media.UserStartNode',
				},
			],
		},
		conditions: [
			{
				alias: UMB_COLLECTION_ALIAS_CONDITION,
				match: UMB_USER_GROUP_COLLECTION_ALIAS,
			},
		],
	},
];
