import type { ManifestSidebarMenuItem } from '@umbraco-cms/models';

const sidebarMenuItem: ManifestSidebarMenuItem = {
	type: 'sidebarMenuItem',
	alias: 'Umb.SidebarMenuItem.Members',
	name: 'Members Sidebar Menu Item',
	weight: 400,
	loader: () => import('./members-sidebar-menu-item.element'),
	meta: {
		label: 'Members',
		icon: 'umb:folder',
		entityType: 'member',
		sidebarMenus: ['Umb.SidebarMenu.Members'],
	},
};

export const manifests = [sidebarMenuItem];
