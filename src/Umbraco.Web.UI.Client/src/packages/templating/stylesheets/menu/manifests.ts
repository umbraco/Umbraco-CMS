import { UMB_STYLESHEET_TREE_ALIAS } from '../tree/index.js';
import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<ManifestTypes> = [
	{
		type: 'menuItem',
		kind: 'tree',
		alias: 'Umb.MenuItem.Stylesheets',
		name: 'Stylesheets Menu Item',
		weight: 20,
		meta: {
			label: 'Stylesheets',
			treeAlias: UMB_STYLESHEET_TREE_ALIAS,
			menus: ['Umb.Menu.Templating'],
		},
	},
	{
		type: 'workspaceContext',
		name: 'Stylesheet Menu Structure Workspace Context',
		alias: 'Umb.Context.Stylesheet.Menu.Structure',
		api: () => import('./stylesheet-menu-structure.context.js'),
		conditions: [
			{
				alias: 'Umb.Condition.WorkspaceAlias',
				match: 'Umb.Workspace.Stylesheet',
			},
		],
	},
	{
		type: 'workspaceFooterApp',
		kind: 'menuBreadcrumb',
		alias: 'Umb.WorkspaceFooterApp.Stylesheet.Breadcrumb',
		name: 'Stylesheet Breadcrumb Workspace Footer App',
		conditions: [
			{
				alias: 'Umb.Condition.WorkspaceAlias',
				match: 'Umb.Workspace.Stylesheet',
			},
		],
	},
];
