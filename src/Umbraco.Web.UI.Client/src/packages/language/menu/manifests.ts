import { UMB_LANGUAGE_ROOT_ENTITY_TYPE } from '../entity.js';
import { UMB_LANGUAGE_WORKSPACE_ALIAS } from '../workspace/language/constants.js';
import { UMB_WORKSPACE_CONDITION_ALIAS } from '@umbraco-cms/backoffice/workspace';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'menuItem',
		alias: 'Umb.MenuItem.Languages',
		name: 'Languages Menu Item',
		weight: 100,
		meta: {
			label: '#treeHeaders_languages',
			icon: 'icon-globe',
			entityType: UMB_LANGUAGE_ROOT_ENTITY_TYPE,
			menus: ['Umb.Menu.StructureSettings'],
		},
	},
	{
		type: 'workspaceContext',
		name: 'Language Menu Structure Workspace Context',
		alias: 'Umb.Context.Language.Menu.Structure',
		api: () => import('./language-menu-structure.context.js'),
		conditions: [
			{
				alias: UMB_WORKSPACE_CONDITION_ALIAS,
				match: UMB_LANGUAGE_WORKSPACE_ALIAS,
			},
		],
	},
	{
		type: 'workspaceFooterApp',
		kind: 'menuBreadcrumb',
		alias: 'Umb.WorkspaceFooterApp.Language.Breadcrumb',
		name: 'Language Breadcrumb Workspace Footer App',
		conditions: [
			{
				alias: UMB_WORKSPACE_CONDITION_ALIAS,
				match: UMB_LANGUAGE_WORKSPACE_ALIAS,
			},
		],
	},
];
