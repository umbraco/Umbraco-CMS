import { UMB_DOCUMENT_TYPE_TREE_ALIAS } from '../tree/index.js';
import { UMB_DOCUMENT_TYPE_MENU_ITEM_ALIAS } from './constants.js';
import { UMB_WORKSPACE_CONDITION_ALIAS } from '@umbraco-cms/backoffice/workspace';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'menuItem',
		kind: 'tree',
		alias: UMB_DOCUMENT_TYPE_MENU_ITEM_ALIAS,
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
		kind: 'menuStructure',
		name: 'Document Type Menu Structure Workspace Context',
		alias: 'Umb.Context.DocumentType.Menu.Structure',
		api: () => import('./document-type-menu-structure.context.js'),
		meta: {
			menuItemAlias: UMB_DOCUMENT_TYPE_MENU_ITEM_ALIAS,
		},
		conditions: [
			{
				alias: UMB_WORKSPACE_CONDITION_ALIAS,
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
				alias: UMB_WORKSPACE_CONDITION_ALIAS,
				match: 'Umb.Workspace.DocumentType',
			},
		],
	},
];
