import type { ManifestMenuItemTreeKind } from '@umbraco-cms/backoffice/extension-registry';

const menuItem: ManifestMenuItemTreeKind = {
	type: 'menuItem',
	kind: 'tree',
	alias: 'Umb.MenuItem.DataTypes',
	name: 'Data Types Menu Item',
	weight: 600,
	meta: {
		label: 'Data Types',
		entityType: 'data-type',
		treeAlias: 'Umb.Tree.DataType',
		menus: ['Umb.Menu.StructureSettings'],
	},
};

export const manifests = [menuItem];
