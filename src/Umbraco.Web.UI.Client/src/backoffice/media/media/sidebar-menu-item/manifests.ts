import type { ManifestSidebarMenuItem } from '@umbraco-cms/models';

const sidebarMenuItem: ManifestSidebarMenuItem = {
	type: 'sidebarMenuItem',
	alias: 'Umb.SidebarMenuItem.Media',
	name: 'Media Sidebar Menu Item',
	weight: 100,
	loader: () => import('./media-sidebar-menu-item.element'),
	meta: {
		label: 'Media',
		icon: 'umb:folder',
		sidebarMenus: ['Umb.SidebarMenu.Media'],
	},
};

export const manifests = [sidebarMenuItem];
