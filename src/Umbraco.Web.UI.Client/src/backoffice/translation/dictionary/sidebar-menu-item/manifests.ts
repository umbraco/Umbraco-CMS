import type { ManifestMenuItem } from '@umbraco-cms/models';

const menuItem: ManifestMenuItem = {
	type: 'menuItem',
	alias: 'Umb.MenuItem.Dictionary',
	name: 'Dictionary Menu Item',
	weight: 400,
	loader: () => import('./dictionary-sidebar-menu-item.element'),
	meta: {
		label: 'Dictionary',
		icon: 'umb:book-alt',
		entityType: 'dictionary-item',
		menus: ['Umb.Menu.Dictionary'],
	},
};

export const manifests = [menuItem];
