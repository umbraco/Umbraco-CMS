import type { ManifestSidebarMenuItem } from '@umbraco-cms/models';

const sidebarMenuItem: ManifestSidebarMenuItem = {
	type: 'sidebarMenuItem',
	alias: 'Umb.SidebarMenuItem.MemberGroups',
	name: 'Member Groups Sidebar Menu Item',
	weight: 800,
	loader: () => import('./member-groups-sidebar-menu-item.element'),
	meta: {
		label: 'Member Groups',
		icon: 'umb:folder',
		sidebarMenus: ['Umb.SidebarMenu.Members'],
	},
};

export const manifests = [sidebarMenuItem];
