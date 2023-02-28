import type { ManifestMenuItem } from '@umbraco-cms/models';

const menuItem: ManifestMenuItem = {
	type: 'menuItem',
	alias: 'Umb.MenuItem.Media',
	name: 'Media Menu Item',
	weight: 100,
	loader: () => import('./media-sidebar-menu-item.element'),
	meta: {
		label: 'Media',
		icon: 'umb:folder',
		menus: ['Umb.Menu.Media'],
	},
};

export const manifests = [menuItem];
