import { SCRIPTS_ENTITY_TYPE, SCRIPTS_MENU_ITEM_ALIAS, SCRIPTS_TREE_ALIAS } from '../config.js';
import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

const menuItem: ManifestTypes = {
	type: 'menuItem',
	kind: 'tree',
	alias: SCRIPTS_MENU_ITEM_ALIAS,
	name: 'Scripts Menu Item',
	weight: 10,
	meta: {
		label: 'Scripts',
		icon: 'umb:folder',
		entityType: SCRIPTS_ENTITY_TYPE,
		treeAlias: SCRIPTS_TREE_ALIAS,
		menus: ['Umb.Menu.Templating'],
	},
};

export const manifests = [menuItem];
