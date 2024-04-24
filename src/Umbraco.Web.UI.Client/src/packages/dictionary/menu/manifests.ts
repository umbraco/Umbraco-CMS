import { UMB_DICTIONARY_ENTITY_TYPE } from '../entity.js';
import { UMB_DICTIONARY_TREE_ALIAS } from '../tree/index.js';
import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

export const UMB_DICTIONARY_MENU_ALIAS = 'Umb.Menu.Dictionary';

export const manifests: Array<ManifestTypes> = [
	{
		type: 'menu',
		alias: UMB_DICTIONARY_MENU_ALIAS,
		name: 'Dictionary Menu',
	},
	{
		type: 'menuItem',
		kind: 'tree',
		alias: 'Umb.MenuItem.Dictionary',
		name: 'Dictionary Menu Item',
		weight: 400,
		meta: {
			label: 'Dictionary',
			icon: 'icon-book-alt',
			entityType: UMB_DICTIONARY_ENTITY_TYPE,
			menus: [UMB_DICTIONARY_MENU_ALIAS],
			treeAlias: UMB_DICTIONARY_TREE_ALIAS,
			hideTreeRoot: true,
		},
	},
	{
		type: 'workspaceContext',
		name: 'Dictionary Menu Structure Workspace Context',
		alias: 'Umb.Context.Dictionary.Menu.Structure',
		api: () => import('./dictionary-menu-structure.context.js'),
		conditions: [
			{
				alias: 'Umb.Condition.WorkspaceAlias',
				match: 'Umb.Workspace.Dictionary',
			},
		],
	},
	{
		type: 'workspaceFooterApp',
		kind: 'menuBreadcrumb',
		alias: 'Umb.WorkspaceFooterApp.Dictionary.Breadcrumb',
		name: 'Data Type Breadcrumb Workspace Footer App',
		conditions: [
			{
				alias: 'Umb.Condition.WorkspaceAlias',
				match: 'Umb.Workspace.Dictionary',
			},
		],
	},
];
