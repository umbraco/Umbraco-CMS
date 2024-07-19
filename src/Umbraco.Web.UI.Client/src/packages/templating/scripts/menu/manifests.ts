import { UMB_SCRIPT_TREE_ALIAS } from '../tree/index.js';
import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

export const UMB_SCRIPT_MENU_ITEM_ALIAS = 'Umb.MenuItem.Script';

export const manifests: Array<ManifestTypes> = [
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
				alias: 'Umb.Condition.WorkspaceAlias',
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
				alias: 'Umb.Condition.WorkspaceAlias',
				match: 'Umb.Workspace.Script',
			},
		],
	},
];
