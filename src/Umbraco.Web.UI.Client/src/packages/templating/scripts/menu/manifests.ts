import { UMB_SCRIPT_TREE_ALIAS } from '../tree/index.js';
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
		name: 'Script Menu Structure Workspace Context',
		alias: 'Umb.Context.Script.Menu.Structure',
		api: () => import('./script-menu-structure.context.js'),
		conditions: [
			{
				alias: UMB_WORKSPACE_CONDITION_ALIAS,
				match: 'Umb.Workspace.Script',
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
				match: 'Umb.Workspace.Script',
			},
		],
	},
];
