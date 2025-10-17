import { UMB_CONTENT_MENU_ALIAS } from '../../menu/manifests.js';
import { UMB_DOCUMENT_RECYCLE_BIN_TREE_ALIAS } from '../constants.js';
import { UMB_DOCUMENT_WORKSPACE_ALIAS } from '../../constants.js';
import { UMB_ENTITY_IS_TRASHED_CONDITION_ALIAS } from '@umbraco-cms/backoffice/recycle-bin';
import { UMB_WORKSPACE_CONDITION_ALIAS } from '@umbraco-cms/backoffice/workspace';
import { UMB_DOCUMENT_RECYCLE_BIN_MENU_ITEM_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'menuItem',
		kind: 'tree',
		alias: UMB_DOCUMENT_RECYCLE_BIN_MENU_ITEM_ALIAS,
		name: 'Document Recycle Bin Menu Item',
		weight: 100,
		meta: {
			treeAlias: UMB_DOCUMENT_RECYCLE_BIN_TREE_ALIAS,
			label: 'Recycle Bin',
			icon: 'icon-trash',
			menus: [UMB_CONTENT_MENU_ALIAS],
		},
		conditions: [
			{
				alias: 'Umb.Condition.CurrentUser.AllowDocumentRecycleBin',
			},
		],
	},
	{
		type: 'workspaceContext',
		kind: 'menuStructure',
		name: 'Document Recycle Bin Menu Structure Workspace Context',
		alias: 'Umb.Context.DocumentRecycleBin.Menu.Structure',
		api: () => import('./document-recycle-bin-menu-structure.context.js'),
		meta: {
			menuItemAlias: UMB_DOCUMENT_RECYCLE_BIN_MENU_ITEM_ALIAS,
		},
		conditions: [
			{
				alias: UMB_WORKSPACE_CONDITION_ALIAS,
				match: UMB_DOCUMENT_WORKSPACE_ALIAS,
			},
			{
				alias: UMB_ENTITY_IS_TRASHED_CONDITION_ALIAS,
			},
		],
	},
];
