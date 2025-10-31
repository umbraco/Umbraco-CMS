import { UMB_DOCUMENT_BLUEPRINT_FOLDER_WORKSPACE_ALIAS, UMB_DOCUMENT_BLUEPRINT_TREE_ALIAS } from '../tree/constants.js';
import { UMB_DOCUMENT_BLUEPRINT_WORKSPACE_ALIAS } from '../workspace/constants.js';
import { UMB_DOCUMENT_BLUEPRINT_MENU_ITEM_ALIAS } from './constants.js';
import { UMB_WORKSPACE_CONDITION_ALIAS } from '@umbraco-cms/backoffice/workspace';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'menuItem',
		kind: 'tree',
		alias: UMB_DOCUMENT_BLUEPRINT_MENU_ITEM_ALIAS,
		name: 'Document Blueprints Menu Item',
		weight: 100,
		meta: {
			treeAlias: UMB_DOCUMENT_BLUEPRINT_TREE_ALIAS,
			label: '#treeHeaders_contentBlueprints',
			menus: ['Umb.Menu.StructureSettings'],
		},
	},
	{
		type: 'workspaceContext',
		kind: 'menuStructure',
		name: 'Document Blueprint Menu Structure Workspace Context',
		alias: 'Umb.Context.DocumentBlueprint.Menu.Structure',
		api: () => import('./document-blueprint-menu-structure.context.js'),
		meta: {
			menuItemAlias: UMB_DOCUMENT_BLUEPRINT_MENU_ITEM_ALIAS,
		},
		conditions: [
			{
				alias: UMB_WORKSPACE_CONDITION_ALIAS,
				match: UMB_DOCUMENT_BLUEPRINT_WORKSPACE_ALIAS,
			},
		],
	},
	{
		type: 'workspaceFooterApp',
		kind: 'menuBreadcrumb',
		alias: 'Umb.WorkspaceFooterApp.DocumentBlueprint.Breadcrumb',
		name: 'Document Blueprint Breadcrumb Workspace Footer App',
		conditions: [
			{
				alias: UMB_WORKSPACE_CONDITION_ALIAS,
				match: UMB_DOCUMENT_BLUEPRINT_WORKSPACE_ALIAS,
			},
		],
	},
	{
		type: 'workspaceContext',
		kind: 'menuStructure',
		name: 'Document Blueprint Folder Menu Structure Workspace Context',
		alias: 'Umb.Context.DocumentBlueprintFolder.Menu.Structure',
		api: () => import('./document-blueprint-menu-structure.context.js'),
		meta: {
			menuItemAlias: UMB_DOCUMENT_BLUEPRINT_MENU_ITEM_ALIAS,
		},
		conditions: [
			{
				alias: UMB_WORKSPACE_CONDITION_ALIAS,
				match: UMB_DOCUMENT_BLUEPRINT_FOLDER_WORKSPACE_ALIAS,
			},
		],
	},
	{
		type: 'workspaceFooterApp',
		kind: 'menuBreadcrumb',
		alias: 'Umb.WorkspaceFooterApp.DocumentBlueprintFolder.Breadcrumb',
		name: 'Document Blueprint Folder Breadcrumb Workspace Footer App',
		conditions: [
			{
				alias: UMB_WORKSPACE_CONDITION_ALIAS,
				match: UMB_DOCUMENT_BLUEPRINT_FOLDER_WORKSPACE_ALIAS,
			},
		],
	},
];
