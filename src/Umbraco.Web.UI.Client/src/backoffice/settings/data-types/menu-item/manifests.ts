import type { ManifestMenuItem } from '@umbraco-cms/models';

const menuItem: ManifestMenuItem = {
	type: 'menuItem',
	alias: 'Umb.MenuItem.DataTypes',
	name: 'Data Types Menu Item',
	weight: 40,
	loader: () => import('./data-types-menu-item.element'),
	meta: {
		label: 'Data Types',
		icon: 'umb:folder',
		entityType: 'data-type',
		menus: ['Umb.Menu.Settings'],
	},
};

export const manifests = [menuItem];
