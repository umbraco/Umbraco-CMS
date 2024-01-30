import { UMB_SCRIPT_TREE_ALIAS } from '../tree/index.js';
import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

export const UMB_SCRIPT_MENU_ITEM_ALIAS = 'Umb.MenuItem.Script';

const menuItem: ManifestTypes = {
	type: 'menuItem',
	kind: 'tree',
	alias: UMB_SCRIPT_MENU_ITEM_ALIAS,
	name: 'Scripts Menu Item',
	weight: 10,
	meta: {
		label: 'Scripts',
		treeAlias: UMB_SCRIPT_TREE_ALIAS,
		menus: ['Umb.Menu.Templating'],
	},
};

export const manifests = [menuItem];
