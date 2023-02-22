import type { ManifestSidebarMenuItem } from '@umbraco-cms/models';

const sidebarMenuItem: ManifestSidebarMenuItem = {
	type: 'sidebarMenuItem',
	alias: 'Umb.SidebarMenuItem.Languages',
	name: 'Languages Sidebar Menu Item',
	weight: 80,
	meta: {
		label: 'Languages',
		icon: 'umb:globe',
		entityType: 'language-root',
		sidebarMenus: ['Umb.SidebarMenu.Settings'],
	},
};

export const manifests = [sidebarMenuItem];
