import type { ManifestSidebarMenuItem } from '@umbraco-cms/models';

const sidebarMenuItem: ManifestSidebarMenuItem = {
	type: 'sidebarMenuItem',
	alias: 'Umb.SidebarMenuItem.LogViewer',
	name: 'LogViewer Sidebar Menu Item',
	weight: 70,
	meta: {
		label: 'Log Viewer',
		icon: 'umb:box-alt',
		entityType: 'logviewer',
		sidebarMenus: ['Umb.SidebarMenu.Settings'],
	},
};

export const manifests = [sidebarMenuItem];
