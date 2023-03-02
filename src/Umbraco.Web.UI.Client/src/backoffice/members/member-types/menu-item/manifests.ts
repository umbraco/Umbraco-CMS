import type { ManifestMenuItem } from '@umbraco-cms/models';

const menuItem: ManifestMenuItem = {
	type: 'menuItem',
	alias: 'Umb.MenuItem.MemberTypes',
	name: 'Member Types Menu Item',
	weight: 30,
	loader: () => import('./member-types-menu-item.element'),
	meta: {
		label: 'Member Types',
		icon: 'umb:folder',
		menus: ['Umb.Menu.Settings'],
	},
};

export const manifests = [menuItem];
