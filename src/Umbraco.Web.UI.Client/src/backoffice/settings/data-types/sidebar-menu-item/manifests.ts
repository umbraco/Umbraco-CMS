import type { ManifestSidebarMenuItem } from '@umbraco-cms/models';

const sidebarMenuItem: ManifestSidebarMenuItem = {
	type: 'sidebarMenuItem',
	alias: 'Umb.SidebarMenuItem.DataTypes',
	name: 'Data Types Sidebar Menu Item',
	weight: 40,
	loader: () => import('./data-types-sidebar-menu-item.element'),
	meta: {
		label: 'Data Types',
		icon: 'umb:folder',
		entityType: 'data-type',
		sidebarMenus: ['Umb.SidebarMenu.Settings'],
	},
};

export const manifests = [sidebarMenuItem];
