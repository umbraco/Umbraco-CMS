import { UMB_PARTIAL_VIEW_TREE_ALIAS } from '../tree/index.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'menuItem',
		kind: 'tree',
		alias: 'Umb.MenuItem.PartialView',
		name: 'Partial View Menu Item',
		weight: 40,
		meta: {
			label: 'Partial Views',
			treeAlias: UMB_PARTIAL_VIEW_TREE_ALIAS,
			menus: ['Umb.Menu.Templating'],
		},
	},
	{
		type: 'workspaceContext',
		name: 'Partial View Menu Structure Workspace Context',
		alias: 'Umb.Context.PartialView.Menu.Structure',
		api: () => import('./partial-view-menu-structure.context.js'),
		conditions: [
			{
				alias: 'Umb.Condition.WorkspaceAlias',
				match: 'Umb.Workspace.PartialView',
			},
		],
	},
	{
		type: 'workspaceFooterApp',
		kind: 'menuBreadcrumb',
		alias: 'Umb.WorkspaceFooterApp.PartialView.Breadcrumb',
		name: 'Partial View Breadcrumb Workspace Footer App',
		conditions: [
			{
				alias: 'Umb.Condition.WorkspaceAlias',
				match: 'Umb.Workspace.PartialView',
			},
		],
	},
];
