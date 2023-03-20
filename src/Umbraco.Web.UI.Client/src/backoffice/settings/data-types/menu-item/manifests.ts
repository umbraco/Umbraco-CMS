import type { ManifestTypes } from '@umbraco-cms/models';

const menuItem: ManifestTypes = {
	type: 'menuItem',
	kind: 'Umb.Kind.Tree',
	alias: 'Umb.MenuItem.DataTypes',
	name: 'Data Types Menu Item',
	weight: 40,
	meta: {
		label: 'Data Types',
		icon: 'umb:folder',
		entityType: 'data-type',
		treeAlias: 'Umb.Tree.DataTypes',
	},
	conditions: {
		menus: ['Umb.Menu.Settings'],
	},
};

export const manifests = [menuItem];
