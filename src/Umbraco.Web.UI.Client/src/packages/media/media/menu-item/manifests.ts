import type { ManifestMenuItem } from '@umbraco-cms/backoffice/extension-registry';

const menuItem: ManifestMenuItem = {
	type: 'menuItem',
	alias: 'Umb.MenuItem.Media',
	name: 'Media Menu Item',
	weight: 100,
	loader: () => import('./media-menu-item.element.js'),
	meta: {
		label: 'Media',
		icon: 'umb:folder',
	},
	conditions: {
		menus: ['Umb.Menu.Media'],
	},
};

export const manifests = [menuItem];
