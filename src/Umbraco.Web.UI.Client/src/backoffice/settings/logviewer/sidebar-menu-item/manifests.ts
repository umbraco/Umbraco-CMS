import type { ManifestSidebarMenuItem } from '@umbraco-cms/models';

const sidebarMenuItem: ManifestSidebarMenuItem = {
	type: 'sidebarMenuItem',
	alias: 'Umb.SidebarMenuItem.LogViewer',
	name: 'LogViewer Sidebar Menu Item',
	weight: 70,
	meta: {
		label: 'Log Viewer',
		icon: 'umb:box-alt',
		sections: ['Umb.Section.Settings'],
		entityType: 'logviewer-root',
	},
};

export const manifests = [sidebarMenuItem];
