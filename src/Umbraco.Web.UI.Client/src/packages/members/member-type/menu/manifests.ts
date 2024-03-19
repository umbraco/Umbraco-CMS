import { UMB_MEMBER_TYPE_TREE_ALIAS } from '../tree/index.js';

export const manifests = [
	{
		type: 'menuItem',
		kind: 'tree',
		alias: 'Umb.MenuItem.MemberTypes',
		name: 'Member Type Menu Item',
		weight: 700,
		meta: {
			label: 'Member Types',
			treeAlias: UMB_MEMBER_TYPE_TREE_ALIAS,
			menus: ['Umb.Menu.Settings'],
		},
	},
	{
		type: 'workspaceContext',
		name: 'Data Type Menu Structure Workspace Context',
		alias: 'Umb.Context.DataType.Menu.Structure',
		api: () => import('./member-type-menu-structure.context.js'),
		conditions: [
			{
				alias: 'Umb.Condition.WorkspaceAlias',
				match: 'Umb.Workspace.DataType',
			},
		],
	},
	{
		type: 'workspaceFooterApp',
		kind: 'breadcrumb',
		alias: 'Umb.WorkspaceFooterApp.DataType.Breadcrumb',
		name: 'Data Type Breadcrumb Workspace Footer App',
		conditions: [
			{
				alias: 'Umb.Condition.WorkspaceAlias',
				match: 'Umb.Workspace.DataType',
			},
		],
	},
];
