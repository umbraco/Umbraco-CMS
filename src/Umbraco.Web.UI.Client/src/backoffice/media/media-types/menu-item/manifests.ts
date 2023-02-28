import type { ManifestMenuItem } from '@umbraco-cms/models';

const menuItem: ManifestMenuItem = {
	type: 'menuItem',
	alias: 'Umb.MenuItem.MediaTypes',
	name: 'Media Types Menu Item',
	weight: 20,
	loader: () => import('./media-types-menu-item.element'),
	meta: {
		label: 'Media Types',
		icon: 'umb:folder',
		menus: ['Umb.Menu.Settings'],
	},
};

export const manifests = [menuItem];
