import { UMB_RELATION_TYPE_TREE_ALIAS } from '../tree/index.js';

export const manifests = [
	{
		type: 'menuItem',
		kind: 'tree',
		alias: 'Umb.MenuItem.RelationTypes',
		name: 'Relation Types Menu Item',
		weight: 500,
		meta: {
			treeAlias: UMB_RELATION_TYPE_TREE_ALIAS,
			label: 'Relation Types',
			menus: ['Umb.Menu.Settings'],
		},
	},
	{
		type: 'workspaceContext',
		name: 'Relation Type Menu Structure Workspace Context',
		alias: 'Umb.Context.RelationType.Menu.Structure',
		api: () => import('./relation-type-menu-structure.context.js'),
		conditions: [
			{
				alias: 'Umb.Condition.WorkspaceAlias',
				match: 'Umb.Workspace.RelationType',
			},
		],
	},
	{
		type: 'workspaceFooterApp',
		kind: 'menbuBreadcrumb',
		alias: 'Umb.WorkspaceFooterApp.RelationType.Breadcrumb',
		name: 'Relation Type Breadcrumb Workspace Footer App',
		conditions: [
			{
				alias: 'Umb.Condition.WorkspaceAlias',
				match: 'Umb.Workspace.RelationType',
			},
		],
	},
];
