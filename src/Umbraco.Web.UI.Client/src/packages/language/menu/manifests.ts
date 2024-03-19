export const manifests = [
	{
		type: 'menuItem',
		alias: 'Umb.MenuItem.Languages',
		name: 'Languages Menu Item',
		weight: 100,
		meta: {
			label: 'Languages',
			icon: 'icon-globe',
			entityType: 'language-root',
			menus: ['Umb.Menu.Settings'],
		},
	},
	{
		type: 'workspaceContext',
		name: 'Language Menu Structure Workspace Context',
		alias: 'Umb.Context.Language.Menu.Structure',
		api: () => import('./language-menu-structure.context.js'),
		conditions: [
			{
				alias: 'Umb.Condition.WorkspaceAlias',
				match: 'Umb.Workspace.Language',
			},
		],
	},
	{
		type: 'workspaceFooterApp',
		kind: 'breadcrumb',
		alias: 'Umb.WorkspaceFooterApp.Language.Breadcrumb',
		name: 'Language Breadcrumb Workspace Footer App',
		conditions: [
			{
				alias: 'Umb.Condition.WorkspaceAlias',
				match: 'Umb.Workspace.Language',
			},
		],
	},
];
