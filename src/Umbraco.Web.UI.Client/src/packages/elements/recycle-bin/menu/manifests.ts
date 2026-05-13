import { UMB_CURRENT_USER_ALLOW_ELEMENT_RECYCLE_BIN_CONDITION_ALIAS } from '../conditions/allow-element-recycle-bin.condition.js';
import { UMB_ELEMENT_MENU_ALIAS } from '../../menu/constants.js';
import { UMB_ELEMENT_WORKSPACE_ALIAS } from '../../workspace/constants.js';
import { UMB_ELEMENT_RECYCLE_BIN_TREE_ALIAS } from '../tree/constants.js';
import { UMB_ELEMENT_RECYCLE_BIN_MENU_ITEM_ALIAS } from './constants.js';
import { UMB_ENTITY_IS_TRASHED_CONDITION_ALIAS } from '@umbraco-cms/backoffice/recycle-bin';
import { UMB_WORKSPACE_CONDITION_ALIAS } from '@umbraco-cms/backoffice/workspace';
import type { ManifestMenuItemTreeKind } from '@umbraco-cms/backoffice/tree';
import type { ManifestWorkspaceContextMenuStructureKind } from '@umbraco-cms/backoffice/menu';

const menuItem: ManifestMenuItemTreeKind = {
	type: 'menuItem',
	kind: 'tree',
	alias: UMB_ELEMENT_RECYCLE_BIN_MENU_ITEM_ALIAS,
	name: 'Element Recycle Bin Menu Item',
	weight: 100,
	meta: {
		label: '#general_recycleBin',
		icon: 'icon-trash',
		menus: [UMB_ELEMENT_MENU_ALIAS],
		treeAlias: UMB_ELEMENT_RECYCLE_BIN_TREE_ALIAS,
	},
	conditions: [{ alias: UMB_CURRENT_USER_ALLOW_ELEMENT_RECYCLE_BIN_CONDITION_ALIAS }],
};

const workspaceContext: ManifestWorkspaceContextMenuStructureKind = {
	type: 'workspaceContext',
	kind: 'menuStructure',
	name: 'Element Recycle Bin Menu Structure Workspace Context',
	alias: 'Umb.Context.ElementRecycleBin.Menu.Structure',
	api: () => import('./element-recycle-bin-menu-structure.context.js'),
	meta: {
		menuItemAlias: UMB_ELEMENT_RECYCLE_BIN_MENU_ITEM_ALIAS,
	},
	conditions: [
		{
			alias: UMB_WORKSPACE_CONDITION_ALIAS,
			match: UMB_ELEMENT_WORKSPACE_ALIAS,
		},
		{
			alias: UMB_ENTITY_IS_TRASHED_CONDITION_ALIAS,
		},
	],
};

export const manifests: Array<UmbExtensionManifest> = [menuItem, workspaceContext];
