import { STYLESHEET_TREE_ALIAS } from '../tree/manifests';
import type { ManifestTypes } from '@umbraco-cms/backoffice/extensions-registry';

const menuItem: ManifestTypes = {
	type: 'menuItem',
	kind: 'tree',
	alias: 'Umb.MenuItem.Stylesheets',
	name: 'Stylesheets Menu Item',
	weight: 400,
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
