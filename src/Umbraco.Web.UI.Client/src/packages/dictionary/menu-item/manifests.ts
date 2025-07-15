import { UMB_DICTIONARY_ENTITY_TYPE } from '../entity.js';
import { UMB_DICTIONARY_TREE_ALIAS } from '../tree/index.js';
import { UMB_WORKSPACE_CONDITION_ALIAS } from '@umbraco-cms/backoffice/workspace';
import { UMB_TRANSLATION_MENU_ALIAS } from '@umbraco-cms/backoffice/translation';

export const manifests: Array<UmbExtensionManifest> = [
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
			menus: [UMB_TRANSLATION_MENU_ALIAS],
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
				alias: UMB_WORKSPACE_CONDITION_ALIAS,
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
				alias: UMB_WORKSPACE_CONDITION_ALIAS,
				match: 'Umb.Workspace.Dictionary',
			},
		],
	},
];
