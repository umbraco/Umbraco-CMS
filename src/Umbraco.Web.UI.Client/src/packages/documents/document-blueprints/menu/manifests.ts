import { UMB_DOCUMENT_BLUEPRINT_TREE_ALIAS } from '../tree/constants.js';
import { UMB_DOCUMENT_BLUEPRINT_ROOT_ENTITY_TYPE } from '../entity.js';
import { UMB_DOCUMENT_BLUEPRINT_MENU_ALIAS, UMB_DOCUMENT_BLUEPRINT_MENU_ITEM_ALIAS } from './constants.js';
import { UMB_CURRENT_USER_DOCUMENT_BLUEPRINT_ACCESS_CONDITION_ALIAS } from '@umbraco-cms/backoffice/current-user';
import { UMB_LIBRARY_SECTION_ALIAS } from '@umbraco-cms/backoffice/library';
import { UMB_SECTION_ALIAS_CONDITION_ALIAS } from '@umbraco-cms/backoffice/section';
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
	alias: 'Umb.SidebarMenu.DocumentBlueprint',
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
		{
			alias: UMB_CURRENT_USER_DOCUMENT_BLUEPRINT_ACCESS_CONDITION_ALIAS,
		},
	],
};

export const manifests: Array<UmbExtensionManifest> = [menu, menuItem, sectionSidebarApp];
