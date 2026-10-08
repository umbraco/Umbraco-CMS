import { UMB_ELEMENT_TREE_ALIAS } from '../tree/constants.js';
import { UMB_ELEMENT_ROOT_ENTITY_TYPE } from '../entity.js';
import { UMB_ELEMENT_MENU_ALIAS, UMB_ELEMENT_MENU_ITEM_ALIAS } from './constants.js';
import { UMB_LIBRARY_SECTION_ALIAS } from '@umbraco-cms/backoffice/library';
import { UMB_SECTION_ALIAS_CONDITION_ALIAS } from '@umbraco-cms/backoffice/section';
import type { ManifestMenu, ManifestSectionSidebarAppMenuWithEntityActionsKind } from '@umbraco-cms/backoffice/menu';
import type { ManifestMenuItemTreeKind } from '@umbraco-cms/backoffice/tree';

const menu: ManifestMenu = {
	type: 'menu',
	alias: UMB_ELEMENT_MENU_ALIAS,
	name: 'Element Menu',
};

const menuItem: ManifestMenuItemTreeKind = {
	type: 'menuItem',
	kind: 'tree',
	alias: UMB_ELEMENT_MENU_ITEM_ALIAS,
	name: 'Element Menu Item',
	weight: 200,
	meta: {
		label: '#general_elements',
		menus: [UMB_ELEMENT_MENU_ALIAS],
		treeAlias: UMB_ELEMENT_TREE_ALIAS,
		hideTreeRoot: true,
	},
};

const sectionSidebarApp: ManifestSectionSidebarAppMenuWithEntityActionsKind = {
	type: 'sectionSidebarApp',
	kind: 'menuWithEntityActions',
	alias: 'Umb.SidebarMenu.Element',
	name: 'Element Sidebar Menu',
	weight: 100,
	meta: {
		label: '#general_elements',
		menu: UMB_ELEMENT_MENU_ALIAS,
		entityType: UMB_ELEMENT_ROOT_ENTITY_TYPE,
	},
	conditions: [
		{
			alias: UMB_SECTION_ALIAS_CONDITION_ALIAS,
			match: UMB_LIBRARY_SECTION_ALIAS,
		},
	],
};

export const manifests: Array<UmbExtensionManifest> = [menu, menuItem, sectionSidebarApp];
