import { UMB_MEMBER_TYPE_TREE_ALIAS } from '../tree/index.js';
import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<ManifestTypes> = [
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
				alias: 'Umb.Condition.WorkspaceAlias',
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
				alias: 'Umb.Condition.WorkspaceAlias',
				match: 'Umb.Workspace.MemberType',
			},
		],
	},
];
