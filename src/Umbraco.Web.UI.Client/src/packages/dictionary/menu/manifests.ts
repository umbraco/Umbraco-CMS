import { UMB_DICTIONARY_ENTITY_TYPE } from '../entity.js';
import { UMB_DICTIONARY_TREE_ALIAS } from '../tree/index.js';
import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

export const UMB_DICTIONARY_MENU_ALIAS = 'Umb.Menu.Dictionary';

const menu: ManifestTypes = {
	type: 'menu',
	alias: UMB_DICTIONARY_MENU_ALIAS,
	name: 'Dictionary Menu',
	meta: {
		label: 'Dictionary',
	},
};

const menuItem: ManifestTypes = {
	type: 'menuItem',
	kind: 'tree',
	alias: 'Umb.MenuItem.Dictionary',
	name: 'Dictionary Menu Item',
	weight: 400,
	meta: {
		label: 'Dictionary',
		icon: 'icon-book-alt',
		entityType: UMB_DICTIONARY_ENTITY_TYPE,
		menus: [UMB_DICTIONARY_MENU_ALIAS],
		treeAlias: UMB_DICTIONARY_TREE_ALIAS,
		hideTreeRoot: true,
	},
};

export const manifests = [menu, menuItem];
