import { UMB_MEMBER_TYPE_TREE_ALIAS } from '../constants.js';
import { UMB_WORKSPACE_CONDITION_ALIAS } from '@umbraco-cms/backoffice/workspace';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'menuItem',
		kind: 'tree',
		alias: 'Umb.MenuItem.MemberTypes',
		name: 'Member Type Menu Item',
		weight: 700,
		meta: {
			label: 'Member Types',
			treeAlias: UMB_MEMBER_TYPE_TREE_ALIAS,
			menus: ['Umb.Menu.StructureSettings'],
		},
	},
	{
		type: 'workspaceContext',
		name: 'Member Type Menu Structure Workspace Context',
		alias: 'Umb.Context.MemberType.Menu.Structure',
		api: () => import('./member-type-menu-structure.context.js'),
		conditions: [
			{
				alias: UMB_WORKSPACE_CONDITION_ALIAS,
				match: 'Umb.Workspace.MemberType',
			},
		],
	},
	{
		type: 'workspaceFooterApp',
		kind: 'menuBreadcrumb',
		alias: 'Umb.WorkspaceFooterApp.MemberType.Breadcrumb',
		name: 'Member Type Breadcrumb Workspace Footer App',
		conditions: [
			{
				alias: UMB_WORKSPACE_CONDITION_ALIAS,
				match: 'Umb.Workspace.MemberType',
			},
		],
	},
];
