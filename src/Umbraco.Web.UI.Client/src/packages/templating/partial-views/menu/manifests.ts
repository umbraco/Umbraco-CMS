import { UMB_PARTIAL_VIEW_WORKSPACE_ALIAS } from '../workspace/constants.js';
import { UMB_PARTIAL_VIEW_FOLDER_WORKSPACE_ALIAS, UMB_PARTIAL_VIEW_TREE_ALIAS } from '../tree/constants.js';
import { UMB_PARTIAL_VIEW_MENU_ITEM_ALIAS } from './constants.js';
import { UMB_WORKSPACE_CONDITION_ALIAS } from '@umbraco-cms/backoffice/workspace';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'menuItem',
		kind: 'tree',
		alias: UMB_PARTIAL_VIEW_MENU_ITEM_ALIAS,
		name: 'Partial View Menu Item',
		weight: 40,
		meta: {
			label: 'Partial Views',
			treeAlias: UMB_PARTIAL_VIEW_TREE_ALIAS,
			menus: ['Umb.Menu.Templating'],
		},
	},
	{
		type: 'workspaceContext',
		kind: 'menuStructure',
		name: 'Partial View Menu Structure Workspace Context',
		alias: 'Umb.Context.PartialView.Menu.Structure',
		api: () => import('./partial-view-menu-structure.context.js'),
		meta: {
			menuItemAlias: UMB_PARTIAL_VIEW_MENU_ITEM_ALIAS,
		},
		conditions: [
			{
				alias: UMB_WORKSPACE_CONDITION_ALIAS,
				match: UMB_PARTIAL_VIEW_WORKSPACE_ALIAS,
			},
		],
	},
	{
		type: 'workspaceFooterApp',
		kind: 'menuBreadcrumb',
		alias: 'Umb.WorkspaceFooterApp.PartialView.Breadcrumb',
		name: 'Partial View Breadcrumb Workspace Footer App',
		conditions: [
			{
				alias: UMB_WORKSPACE_CONDITION_ALIAS,
				match: UMB_PARTIAL_VIEW_WORKSPACE_ALIAS,
			},
		],
	},
	{
		type: 'workspaceContext',
		kind: 'menuStructure',
		name: 'Partial View Folder Menu Structure Workspace Context',
		alias: 'Umb.Context.PartialViewFolder.Menu.Structure',
		api: () => import('./partial-view-menu-structure.context.js'),
		meta: {
			menuItemAlias: UMB_PARTIAL_VIEW_MENU_ITEM_ALIAS,
		},
		conditions: [
			{
				alias: UMB_WORKSPACE_CONDITION_ALIAS,
				match: UMB_PARTIAL_VIEW_FOLDER_WORKSPACE_ALIAS,
			},
		],
	},
	{
		type: 'workspaceFooterApp',
		kind: 'menuBreadcrumb',
		alias: 'Umb.WorkspaceFooterApp.PartialViewFolder.Breadcrumb',
		name: 'Partial View Folder Breadcrumb Workspace Footer App',
		conditions: [
			{
				alias: UMB_WORKSPACE_CONDITION_ALIAS,
				match: UMB_PARTIAL_VIEW_FOLDER_WORKSPACE_ALIAS,
			},
		],
	},
];
