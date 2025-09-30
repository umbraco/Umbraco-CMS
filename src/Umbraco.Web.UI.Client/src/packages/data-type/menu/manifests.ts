import { UMB_DATA_TYPE_MENU_ITEM_ALIAS } from './constants.js';
import { UMB_WORKSPACE_CONDITION_ALIAS } from '@umbraco-cms/backoffice/workspace';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'menuItem',
		kind: 'tree',
		alias: UMB_DATA_TYPE_MENU_ITEM_ALIAS,
		name: 'Data Types Menu Item',
		weight: 600,
		meta: {
			label: 'Data Types',
			entityType: 'data-type',
			treeAlias: 'Umb.Tree.DataType',
			menus: ['Umb.Menu.StructureSettings'],
		},
	},
	{
		type: 'workspaceContext',
		kind: 'menuStructure',
		name: 'Data Type Menu Structure Workspace Context',
		alias: 'Umb.Context.DataType.Menu.Structure',
		api: () => import('./data-type-menu-structure.context.js'),
		meta: {
			menuItemAlias: UMB_DATA_TYPE_MENU_ITEM_ALIAS,
		},
		conditions: [
			{
				alias: UMB_WORKSPACE_CONDITION_ALIAS,
				match: 'Umb.Workspace.DataType',
			},
		],
	},
	{
		type: 'workspaceFooterApp',
		kind: 'menuBreadcrumb',
		alias: 'Umb.WorkspaceFooterApp.DataType.Breadcrumb',
		name: 'Data Type Breadcrumb Workspace Footer App',
		conditions: [
			{
				alias: UMB_WORKSPACE_CONDITION_ALIAS,
				match: 'Umb.Workspace.DataType',
			},
		],
	},
];
