import { UMB_DOCUMENT_BLUEPRINT_TREE_ALIAS } from '../tree/constants.js';
import { UMB_DOCUMENT_BLUEPRINT_ROOT_ENTITY_TYPE } from '../entity.js';
import { UMB_DOCUMENT_BLUEPRINT_WORKSPACE_ALIAS } from '../workspace/constants.js';
import {
	UMB_DOCUMENT_BLUEPRINT_MENU_ALIAS,
	UMB_DOCUMENT_BLUEPRINT_MENU_ITEM_ALIAS,
	UMB_DOCUMENT_BLUEPRINT_SIDEBAR_APP_ALIAS,
} from './constants.js';
import { UMB_LIBRARY_SECTION_ALIAS } from '@umbraco-cms/backoffice/library';
import { UMB_SECTION_ALIAS_CONDITION_ALIAS } from '@umbraco-cms/backoffice/section';
import { UMB_WORKSPACE_CONDITION_ALIAS } from '@umbraco-cms/backoffice/workspace';
import type { ManifestMenu, ManifestSectionSidebarAppMenuWithEntityActionsKind } from '@umbraco-cms/backoffice/menu';
import type { ManifestMenuItemTreeKind } from '@umbraco-cms/backoffice/tree';

const menu: ManifestMenu = {
	type: 'menu',
	alias: UMB_DOCUMENT_BLUEPRINT_MENU_ALIAS,
	name: 'Document Blueprint Menu',
};

const menuItem: ManifestMenuItemTreeKind = {
	type: 'menuItem',
	kind: 'tree',
	alias: UMB_DOCUMENT_BLUEPRINT_MENU_ITEM_ALIAS,
	name: 'Document Blueprints Menu Item',
	weight: 100,
	meta: {
		treeAlias: UMB_DOCUMENT_BLUEPRINT_TREE_ALIAS,
		label: '#treeHeaders_contentBlueprints',
		menus: [UMB_DOCUMENT_BLUEPRINT_MENU_ALIAS],
		hideTreeRoot: true,
	},
};

const sectionSidebarApp: ManifestSectionSidebarAppMenuWithEntityActionsKind = {
	type: 'sectionSidebarApp',
	kind: 'menuWithEntityActions',
	alias: UMB_DOCUMENT_BLUEPRINT_SIDEBAR_APP_ALIAS,
	name: 'Document Blueprint Sidebar Menu',
	weight: 90,
	meta: {
		label: '#treeHeaders_contentBlueprints',
		menu: UMB_DOCUMENT_BLUEPRINT_MENU_ALIAS,
		entityType: UMB_DOCUMENT_BLUEPRINT_ROOT_ENTITY_TYPE,
	},
	conditions: [
		{
			alias: UMB_SECTION_ALIAS_CONDITION_ALIAS,
			match: UMB_LIBRARY_SECTION_ALIAS,
		},
	],
};

const menuStructureWorkspaceContext: UmbExtensionManifest = {
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
};

const breadcrumbWorkspaceFooterApp: UmbExtensionManifest = {
	type: 'workspaceFooterApp',
	kind: 'variantMenuBreadcrumb',
	alias: 'Umb.WorkspaceFooterApp.DocumentBlueprint.Breadcrumb',
	name: 'Document Blueprint Breadcrumb Workspace Footer App',
	conditions: [
		{
			alias: UMB_WORKSPACE_CONDITION_ALIAS,
			match: UMB_DOCUMENT_BLUEPRINT_WORKSPACE_ALIAS,
		},
	],
};

export const manifests: Array<UmbExtensionManifest> = [
	menu,
	menuItem,
	sectionSidebarApp,
	menuStructureWorkspaceContext,
	breadcrumbWorkspaceFooterApp,
];
