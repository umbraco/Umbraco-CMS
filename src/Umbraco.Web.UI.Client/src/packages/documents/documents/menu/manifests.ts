import { UMB_DOCUMENT_TREE_ALIAS } from '../tree/index.js';

export const UMB_CONTENT_MENU_ALIAS = 'Umb.Menu.Content';

export const manifests = [
	{
		type: 'menu',
		alias: UMB_CONTENT_MENU_ALIAS,
		name: 'Content Menu',
		meta: {
			label: 'Content',
		},
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
		name: 'Document Structure Context',
		alias: 'Umb.Context.Document.Structure',
		api: () => import('./document-menu-structure.context.js'),
		conditions: [
			{
				alias: 'Umb.Condition.WorkspaceAlias',
				match: 'Umb.Workspace.Document',
			},
		],
	},
];
