import type { ManifestMenuItem } from '@umbraco-cms/models';

const menuItem: ManifestMenuItem = {
	type: 'menuItem',
	alias: 'Umb.MenuItem.MemberGroups',
	name: 'Member Groups Menu Item',
	weight: 800,
	loader: () => import('./member-groups-sidebar-menu-item.element'),
	meta: {
		label: 'Member Groups',
		icon: 'umb:folder',
		menus: ['Umb.Menu.Members'],
	},
};

export const manifests = [menuItem];
