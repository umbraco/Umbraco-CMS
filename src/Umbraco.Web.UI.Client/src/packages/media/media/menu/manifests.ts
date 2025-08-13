import { UMB_MEDIA_TREE_ALIAS } from '../constants.js';
import { UMB_MEDIA_MENU_ALIAS, UMB_MEDIA_MENU_ITEM_ALIAS } from './constants.js';
import { UMB_WORKSPACE_CONDITION_ALIAS } from '@umbraco-cms/backoffice/workspace';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'menu',
		alias: UMB_MEDIA_MENU_ALIAS,
		name: 'Media Menu',
	},
	{
		type: 'menuItem',
		kind: 'tree',
		alias: UMB_MEDIA_MENU_ITEM_ALIAS,
		name: 'Media Menu Item',
		weight: 100,
		meta: {
			label: 'Media',
			menus: [UMB_MEDIA_MENU_ALIAS],
			treeAlias: UMB_MEDIA_TREE_ALIAS,
			hideTreeRoot: true,
		},
	},
	{
		type: 'workspaceContext',
		kind: 'menuStructure',
		name: 'Media Menu Structure Workspace Context',
		alias: 'Umb.Context.Media.Menu.Structure',
		api: () => import('./media-menu-structure.context.js'),
		meta: {
			menuItemAlias: UMB_MEDIA_MENU_ITEM_ALIAS,
		},
		conditions: [
			{
				alias: UMB_WORKSPACE_CONDITION_ALIAS,
				match: 'Umb.Workspace.Media',
			},
		],
	},
	{
		type: 'workspaceFooterApp',
		kind: 'variantMenuBreadcrumb',
		alias: 'Umb.WorkspaceFooterApp.Media.Breadcrumb',
		name: 'Media Breadcrumb Workspace Footer App',
		conditions: [
			{
				alias: UMB_WORKSPACE_CONDITION_ALIAS,
				match: 'Umb.Workspace.Media',
			},
		],
	},
];
