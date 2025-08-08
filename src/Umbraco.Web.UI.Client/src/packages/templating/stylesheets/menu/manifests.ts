import { UMB_STYLESHEET_TREE_ALIAS } from '../constants.js';
import { UMB_STYLESHEET_MENU_ITEM_ALIAS } from './constants.js';
import { UMB_WORKSPACE_CONDITION_ALIAS } from '@umbraco-cms/backoffice/workspace';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'menuItem',
		kind: 'tree',
		alias: UMB_STYLESHEET_MENU_ITEM_ALIAS,
		name: 'Stylesheets Menu Item',
		weight: 20,
		meta: {
			label: 'Stylesheets',
			treeAlias: UMB_STYLESHEET_TREE_ALIAS,
			menus: ['Umb.Menu.Templating'],
		},
	},
	{
		type: 'workspaceContext',
		kind: 'menuStructure',
		name: 'Stylesheet Menu Structure Workspace Context',
		alias: 'Umb.Context.Stylesheet.Menu.Structure',
		api: () => import('./stylesheet-menu-structure.context.js'),
		meta: {
			menuItemAlias: UMB_STYLESHEET_MENU_ITEM_ALIAS,
		},
		conditions: [
			{
				alias: UMB_WORKSPACE_CONDITION_ALIAS,
				match: 'Umb.Workspace.Stylesheet',
			},
		],
	},
	{
		type: 'workspaceFooterApp',
		kind: 'menuBreadcrumb',
		alias: 'Umb.WorkspaceFooterApp.Stylesheet.Breadcrumb',
		name: 'Stylesheet Breadcrumb Workspace Footer App',
		conditions: [
			{
				alias: UMB_WORKSPACE_CONDITION_ALIAS,
				match: 'Umb.Workspace.Stylesheet',
			},
		],
	},
];
