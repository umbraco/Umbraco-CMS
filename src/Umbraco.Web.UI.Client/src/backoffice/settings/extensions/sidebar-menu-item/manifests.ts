import type { ManifestSidebarMenuItem } from '@umbraco-cms/models';

const sidebarMenuItem: ManifestSidebarMenuItem = {
	type: 'sidebarMenuItem',
	alias: 'Umb.SidebarMenuItem.Extensions',
	name: 'Extensions Sidebar Menu Item',
	weight: 100,
	meta: {
		label: 'Extensions',
		icon: 'umb:wand',
		entityType: 'extension-root',
		sidebarMenus: ['Umb.SidebarMenu.Settings'],
	},
};

export const manifests = [sidebarMenuItem];
