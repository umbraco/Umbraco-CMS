import type { ManifestSidebarMenuItem } from '@umbraco-cms/models';

const sidebarMenuItem: ManifestSidebarMenuItem = {
	type: 'sidebarMenuItem',
	alias: 'Umb.SidebarMenuItem.MediaTypes',
	name: 'Media Types Sidebar Menu Item',
	weight: 20,
	loader: () => import('./media-types-sidebar-menu-item.element'),
	meta: {
		label: 'Media Types',
		icon: 'umb:folder',
		sidebarMenus: ['Umb.SidebarMenu.Settings'],
	},
};

export const manifests = [sidebarMenuItem];
