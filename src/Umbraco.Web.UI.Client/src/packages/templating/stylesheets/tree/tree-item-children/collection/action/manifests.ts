import { UMB_STYLESHEET_ENTITY_TYPE } from '../../../../entity.js';
import { UMB_STYLESHEET_TREE_ITEM_CHILDREN_COLLECTION_ALIAS } from '../constants.js';
import { UMB_COLLECTION_ALIAS_CONDITION } from '@umbraco-cms/backoffice/collection';
import { UMB_SCHEMA_OPERATION_ALLOWED_CONDITION_ALIAS } from '@umbraco-cms/backoffice/schema-lockdown';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'collectionAction',
		kind: 'create',
		name: 'Stylesheet Tree Item Children Collection Create Action',
		alias: 'Umb.CollectionAction.StylesheetTreeItemChildren.Create',
		conditions: [
			{
				alias: UMB_COLLECTION_ALIAS_CONDITION,
				match: UMB_STYLESHEET_TREE_ITEM_CHILDREN_COLLECTION_ALIAS,
			},
			{
				alias: UMB_SCHEMA_OPERATION_ALLOWED_CONDITION_ALIAS,
				entityType: UMB_STYLESHEET_ENTITY_TYPE,
				operation: 'create',
			},
		],
	},
];
