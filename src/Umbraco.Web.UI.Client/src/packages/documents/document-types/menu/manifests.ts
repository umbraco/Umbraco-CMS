import { UMB_DOCUMENT_TYPE_TREE_ALIAS } from '../tree/index.js';
import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<ManifestTypes> = [
	{
		type: 'menuItem',
		kind: 'tree',
		alias: 'Umb.MenuItem.DocumentTypes',
		name: 'Document Types Menu Item',
		weight: 900,
		meta: {
			treeAlias: UMB_DOCUMENT_TYPE_TREE_ALIAS,
			label: 'Document Types',
			menus: ['Umb.Menu.StructureSettings'],
		},
	},
	{
		type: 'workspaceContext',
		name: 'Document Type Menu Structure Workspace Context',
		alias: 'Umb.Context.DocumentType.Menu.Structure',
		api: () => import('./document-type-menu-structure.context.js'),
		conditions: [
			{
				alias: 'Umb.Condition.WorkspaceAlias',
				match: 'Umb.Workspace.DocumentType',
			},
		],
	},
	{
		type: 'workspaceFooterApp',
		kind: 'menuBreadcrumb',
		alias: 'Umb.WorkspaceFooterApp.DocumentType.Breadcrumb',
		name: 'Document Type Breadcrumb Workspace Footer App',
		conditions: [
			{
				alias: 'Umb.Condition.WorkspaceAlias',
				match: 'Umb.Workspace.DocumentType',
			},
		],
	},
];
