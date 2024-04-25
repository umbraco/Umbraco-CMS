import { UMB_DOCUMENT_TREE_ALIAS } from '../tree/index.js';
import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

export const UMB_CONTENT_MENU_ALIAS = 'Umb.Menu.Content';

export const manifests: Array<ManifestTypes> = [
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
				alias: 'Umb.Condition.WorkspaceAlias',
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
				alias: 'Umb.Condition.WorkspaceAlias',
				match: 'Umb.Workspace.Document',
			},
		],
	},
];
