import { UMB_MEDIA_TYPE_TREE_ALIAS, UMB_MEDIA_TYPE_FOLDER_WORKSPACE_ALIAS } from '../tree/constants.js';
import { UMB_MEDIA_TYPE_WORKSPACE_ALIAS } from '../workspace/constants.js';
import { UMB_MEDIA_TYPE_MENU_ITEM_ALIAS } from './constants.js';
import { UMB_WORKSPACE_CONDITION_ALIAS } from '@umbraco-cms/backoffice/workspace';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'menuItem',
		kind: 'tree',
		alias: UMB_MEDIA_TYPE_MENU_ITEM_ALIAS,
		name: 'Media Types Menu Item',
		weight: 800,
		meta: {
			label: 'Media Types',
			treeAlias: UMB_MEDIA_TYPE_TREE_ALIAS,
			menus: ['Umb.Menu.StructureSettings'],
		},
	},
	{
		type: 'workspaceContext',
		kind: 'menuStructure',
		name: 'Media Type Menu Structure Workspace Context',
		alias: 'Umb.Context.MediaType.Menu.Structure',
		api: () => import('./media-type-menu-structure.context.js'),
		meta: {
			menuItemAlias: UMB_MEDIA_TYPE_MENU_ITEM_ALIAS,
		},
		conditions: [
			{
				alias: UMB_WORKSPACE_CONDITION_ALIAS,
				match: UMB_MEDIA_TYPE_WORKSPACE_ALIAS,
			},
		],
	},
	{
		type: 'workspaceFooterApp',
		kind: 'menuBreadcrumb',
		alias: 'Umb.WorkspaceFooterApp.MediaType.Breadcrumb',
		name: 'Media Type Breadcrumb Workspace Footer App',
		conditions: [
			{
				alias: UMB_WORKSPACE_CONDITION_ALIAS,
				match: UMB_MEDIA_TYPE_WORKSPACE_ALIAS,
			},
		],
	},
	{
		type: 'workspaceContext',
		kind: 'menuStructure',
		name: 'Media Type Folder Menu Structure Workspace Context',
		alias: 'Umb.Context.MediaTypeFolder.Menu.Structure',
		api: () => import('./media-type-menu-structure.context.js'),
		meta: {
			menuItemAlias: UMB_MEDIA_TYPE_MENU_ITEM_ALIAS,
		},
		conditions: [
			{
				alias: UMB_WORKSPACE_CONDITION_ALIAS,
				match: UMB_MEDIA_TYPE_FOLDER_WORKSPACE_ALIAS,
			},
		],
	},
	{
		type: 'workspaceFooterApp',
		kind: 'menuBreadcrumb',
		alias: 'Umb.WorkspaceFooterApp.MediaTypeFolder.Breadcrumb',
		name: 'Media Type Folder Breadcrumb Workspace Footer App',
		conditions: [
			{
				alias: UMB_WORKSPACE_CONDITION_ALIAS,
				match: UMB_MEDIA_TYPE_FOLDER_WORKSPACE_ALIAS,
			},
		],
	},
];
