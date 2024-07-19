import { UMB_MEDIA_TYPE_TREE_ALIAS } from '../tree/index.js';
import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<ManifestTypes> = [
	{
		type: 'menuItem',
		kind: 'tree',
		alias: 'Umb.MenuItem.MediaTypes',
		name: 'Media Types Menu Item',
		weight: 800,
		meta: {
			label: 'Media Types',
			treeAlias: UMB_MEDIA_TYPE_TREE_ALIAS,
			menus: ['Umb.Menu.StructureSettings'],
		},
	},
	{
		type: 'workspaceContext',
		name: 'Media Type Menu Structure Workspace Context',
		alias: 'Umb.Context.MediaType.Menu.Structure',
		api: () => import('./media-type-menu-structure.context.js'),
		conditions: [
			{
				alias: 'Umb.Condition.WorkspaceAlias',
				match: 'Umb.Workspace.MediaType',
			},
		],
	},
	{
		type: 'workspaceFooterApp',
		kind: 'menuBreadcrumb',
		alias: 'Umb.WorkspaceFooterApp.MediaType.Breadcrumb',
		name: 'Media Type Breadcrumb Workspace Footer App',
		conditions: [
			{
				alias: 'Umb.Condition.WorkspaceAlias',
				match: 'Umb.Workspace.MediaType',
			},
		],
	},
];
