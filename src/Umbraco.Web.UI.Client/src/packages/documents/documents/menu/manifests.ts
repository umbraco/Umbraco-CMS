import { UMB_DOCUMENT_TREE_ALIAS } from '../tree/index.js';
import { UMB_WORKSPACE_CONDITION_ALIAS } from '@umbraco-cms/backoffice/workspace';

export const UMB_CONTENT_MENU_ALIAS = 'Umb.Menu.Content';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'menu',
		alias: UMB_CONTENT_MENU_ALIAS,
		name: 'Content Menu',
	},
	{
		type: 'menuItem',
		kind: 'tree',
		alias: 'Umb.MenuItem.Document',
		name: 'Document Menu Item',
		weight: 200,
		meta: {
			label: 'Documents',
			menus: [UMB_CONTENT_MENU_ALIAS],
			treeAlias: UMB_DOCUMENT_TREE_ALIAS,
			hideTreeRoot: true,
		},
	},
	{
		type: 'workspaceContext',
		name: 'Document Menu Structure Workspace Context',
		alias: 'Umb.Context.Document.Menu.Structure',
		api: () => import('./document-menu-structure.context.js'),
		conditions: [
			{
				alias: UMB_WORKSPACE_CONDITION_ALIAS,
				match: 'Umb.Workspace.Document',
			},
		],
	},
	{
		type: 'workspaceFooterApp',
		kind: 'variantMenuBreadcrumb',
		alias: 'Umb.WorkspaceFooterApp.Document.Breadcrumb',
		name: 'Document Breadcrumb Workspace Footer App',
		conditions: [
			{
				alias: UMB_WORKSPACE_CONDITION_ALIAS,
				match: 'Umb.Workspace.Document',
			},
		],
	},
];
