import { UMB_SCRIPT_WORKSPACE_ALIAS } from '../workspace/constants.js';
import { UMB_SCRIPT_TREE_ALIAS } from '../tree/index.js';
import { UMB_SCRIPT_FOLDER_WORKSPACE_ALIAS } from '../tree/constants.js';
import { UMB_SCRIPT_MENU_ITEM_ALIAS } from './constants.js';
import { UMB_WORKSPACE_CONDITION_ALIAS } from '@umbraco-cms/backoffice/workspace';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'menuItem',
		kind: 'tree',
		alias: UMB_SCRIPT_MENU_ITEM_ALIAS,
		name: 'Scripts Menu Item',
		weight: 10,
		meta: {
			label: 'Scripts',
			treeAlias: UMB_SCRIPT_TREE_ALIAS,
			menus: ['Umb.Menu.Templating'],
		},
	},
	{
		type: 'workspaceContext',
		kind: 'menuStructure',
		name: 'Script Menu Structure Workspace Context',
		alias: 'Umb.Context.Script.Menu.Structure',
		api: () => import('./script-menu-structure.context.js'),
		meta: {
			menuItemAlias: UMB_SCRIPT_MENU_ITEM_ALIAS,
		},
		conditions: [
			{
				alias: UMB_WORKSPACE_CONDITION_ALIAS,
				match: UMB_SCRIPT_WORKSPACE_ALIAS,
			},
		],
	},
	{
		type: 'workspaceFooterApp',
		kind: 'menuBreadcrumb',
		alias: 'Umb.WorkspaceFooterApp.Script.Breadcrumb',
		name: 'Script Breadcrumb Workspace Footer App',
		conditions: [
			{
				alias: UMB_WORKSPACE_CONDITION_ALIAS,
				match: UMB_SCRIPT_WORKSPACE_ALIAS,
			},
		],
	},
	{
		type: 'workspaceContext',
		kind: 'menuStructure',
		name: 'Script Folder Menu Structure Workspace Context',
		alias: 'Umb.Context.ScriptFolder.Menu.Structure',
		api: () => import('./script-menu-structure.context.js'),
		meta: {
			menuItemAlias: UMB_SCRIPT_MENU_ITEM_ALIAS,
		},
		conditions: [
			{
				alias: UMB_WORKSPACE_CONDITION_ALIAS,
				match: UMB_SCRIPT_FOLDER_WORKSPACE_ALIAS,
			},
		],
	},
	{
		type: 'workspaceFooterApp',
		kind: 'menuBreadcrumb',
		alias: 'Umb.WorkspaceFooterApp.ScriptFolder.Breadcrumb',
		name: 'Script Folder Breadcrumb Workspace Footer App',
		conditions: [
			{
				alias: UMB_WORKSPACE_CONDITION_ALIAS,
				match: UMB_SCRIPT_FOLDER_WORKSPACE_ALIAS,
			},
		],
	},
];
