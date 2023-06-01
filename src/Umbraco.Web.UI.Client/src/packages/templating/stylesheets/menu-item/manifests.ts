import { STYLESHEET_TREE_ALIAS } from '../tree/manifests.js';
import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

const menuItem: ManifestTypes = {
	type: 'menuItem',
	kind: 'tree',
	alias: 'Umb.MenuItem.Stylesheets',
	name: 'Stylesheets Menu Item',
	weight: 20,
	meta: {
		label: 'Stylesheets',
		icon: 'umb:folder',
		treeAlias: STYLESHEET_TREE_ALIAS,
	},
	conditions: {
		menus: ['Umb.Menu.Templating'],
	},
};

export const manifests = [menuItem];
