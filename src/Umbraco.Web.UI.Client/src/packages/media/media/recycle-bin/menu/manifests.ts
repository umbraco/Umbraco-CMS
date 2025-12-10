import { UMB_MEDIA_MENU_ALIAS, UMB_MEDIA_RECYCLE_BIN_TREE_ALIAS, UMB_MEDIA_WORKSPACE_ALIAS } from '../../constants.js';
import { UMB_MEDIA_RECYCLE_BIN_MENU_ITEM_ALIAS } from './constants.js';
import { UMB_WORKSPACE_CONDITION_ALIAS } from '@umbraco-cms/backoffice/workspace';
import { UMB_ENTITY_IS_TRASHED_CONDITION_ALIAS } from '@umbraco-cms/backoffice/recycle-bin';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'menuItem',
		kind: 'tree',
		alias: UMB_MEDIA_RECYCLE_BIN_MENU_ITEM_ALIAS,
		name: 'Media Recycle Bin Menu Item',
		weight: 100,
		meta: {
			treeAlias: UMB_MEDIA_RECYCLE_BIN_TREE_ALIAS,
			label: 'Recycle Bin',
			icon: 'icon-trash',
			menus: [UMB_MEDIA_MENU_ALIAS],
		},
		conditions: [
			{
				alias: 'Umb.Condition.CurrentUser.AllowMediaRecycleBin',
			},
		],
	},
	{
		type: 'workspaceContext',
		kind: 'menuStructure',
		name: 'Media Recycle Bin Menu Structure Workspace Context',
		alias: 'Umb.Context.MediaRecycleBin.Menu.Structure',
		api: () => import('./media-recycle-bin-menu-structure.context.js'),
		meta: {
			menuItemAlias: UMB_MEDIA_RECYCLE_BIN_MENU_ITEM_ALIAS,
		},
		conditions: [
			{
				alias: UMB_WORKSPACE_CONDITION_ALIAS,
				match: UMB_MEDIA_WORKSPACE_ALIAS,
			},
			{
				alias: UMB_ENTITY_IS_TRASHED_CONDITION_ALIAS,
			},
		],
	},
];
