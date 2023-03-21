import type { ManifestTypes } from '@umbraco-cms/backoffice/extensions-registry';

const menuItem: ManifestTypes = {
	type: 'menuItem',
	kind: 'tree',
	alias: 'Umb.MenuItem.MediaTypes',
	name: 'Media Types Menu Item',
	weight: 20,
	meta: {
		label: 'Media Types',
		icon: 'umb:folder',
		treeAlias: 'Umb.Tree.MediaTypes',
	},
	conditions: {
		menus: ['Umb.Menu.Settings'],
	},
};

export const manifests = [menuItem];
