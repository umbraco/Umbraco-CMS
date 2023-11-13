import type { ManifestMenuItem } from '@umbraco-cms/backoffice/extension-registry';

const menuItem: ManifestMenuItem = {
	type: 'menuItem',
	alias: 'Umb.MenuItem.Media',
	name: 'Media Menu Item',
	weight: 100,
	js: () => import('./media-menu-item.element.js'),
	meta: {
		label: 'Media',
		icon: 'icon-folder',
		menus: ['Umb.Menu.Media'],
	},
};

export const manifests = [menuItem];
