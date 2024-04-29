import { UMB_TEMPLATE_TREE_ALIAS } from '../tree/index.js';
import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<ManifestTypes> = [
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
				alias: 'Umb.Condition.WorkspaceAlias',
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
				alias: 'Umb.Condition.WorkspaceAlias',
				match: 'Umb.Workspace.Template',
			},
		],
	},
];
