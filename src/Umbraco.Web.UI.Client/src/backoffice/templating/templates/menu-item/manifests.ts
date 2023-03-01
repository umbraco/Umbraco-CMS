import type { ManifestMenuItem } from '@umbraco-cms/models';

const menuItem: ManifestMenuItem = {
	type: 'menuItem',
	alias: 'Umb.MenuItem.Templates',
	name: 'Templates Menu Item',
	weight: 40,
	loader: () => import('./templates-menu-item.element'),
	meta: {
		label: 'Templates',
		icon: 'umb:folder',
		entityType: 'template',
		menus: ['Umb.Menu.Templating'],
	},
};

export const manifests = [menuItem];
