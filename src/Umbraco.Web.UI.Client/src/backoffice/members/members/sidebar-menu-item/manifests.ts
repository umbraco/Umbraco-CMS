import type { ManifestMenuItem } from '@umbraco-cms/models';

const menuItem: ManifestMenuItem = {
	type: 'menuItem',
	alias: 'Umb.MenuItem.Members',
	name: 'Members Menu Item',
	weight: 400,
	loader: () => import('./members-sidebar-menu-item.element'),
	meta: {
		label: 'Members',
		icon: 'umb:folder',
		entityType: 'member',
		menus: ['Umb.Menu.Members'],
	},
};

export const manifests = [menuItem];
