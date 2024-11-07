import { UMB_TEMPLATE_TREE_ALIAS } from '../tree/index.js';
import { UMB_WORKSPACE_CONDITION_ALIAS } from '@umbraco-cms/backoffice/workspace';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'menuItem',
		kind: 'tree',
		alias: 'Umb.MenuItem.Templates',
		name: 'Templates Menu Item',
		weight: 40,
		meta: {
			label: 'Templates',
			entityType: 'template',
			treeAlias: UMB_TEMPLATE_TREE_ALIAS,
			menus: ['Umb.Menu.Templating'],
		},
	},
	{
		type: 'workspaceContext',
		name: 'Template Menu Structure Workspace Context',
		alias: 'Umb.Context.Template.Menu.Structure',
		api: () => import('./template-menu-structure.context.js'),
		conditions: [
			{
				alias: UMB_WORKSPACE_CONDITION_ALIAS,
				match: 'Umb.Workspace.Template',
			},
		],
	},
	{
		type: 'workspaceFooterApp',
		kind: 'menuBreadcrumb',
		alias: 'Umb.WorkspaceFooterApp.Template.Breadcrumb',
		name: 'Template Breadcrumb Workspace Footer App',
		conditions: [
			{
				alias: UMB_WORKSPACE_CONDITION_ALIAS,
				match: 'Umb.Workspace.Template',
			},
		],
	},
];
