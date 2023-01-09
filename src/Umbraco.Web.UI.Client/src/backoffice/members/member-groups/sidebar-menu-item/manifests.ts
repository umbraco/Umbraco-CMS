import type { ManifestSidebarMenuItem } from '@umbraco-cms/models';

const sidebarMenuItem: ManifestSidebarMenuItem = {
	type: 'sidebarMenuItem',
	alias: 'Umb.SidebarMenuItem.MemberGroups',
	name: 'Member Groups Sidebar Menu Item',
	weight: 400,
	loader: () => import('./member-groups-sidebar-menu-item.element'),
	meta: {
		label: 'Member Groups',
		icon: 'umb:folder',
		sections: ['Umb.Section.Members'],
	},
};

export const manifests = [sidebarMenuItem];
