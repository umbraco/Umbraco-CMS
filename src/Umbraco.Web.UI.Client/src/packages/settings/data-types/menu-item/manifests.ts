import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

const menuItem: ManifestTypes = {
	type: 'menuItem',
	kind: 'tree',
	alias: 'Umb.MenuItem.DataTypes',
	name: 'Data Types Menu Item',
	weight: 600,
	meta: {
		label: 'Data Types',
		icon: 'umb:folder',
		entityType: 'data-type',
		treeAlias: 'Umb.Tree.DataTypes',
		menus: ['Umb.Menu.Settings'],
	},
};

export const manifests = [menuItem];
