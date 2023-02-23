import type { ManifestSidebarMenuItem } from '@umbraco-cms/models';

const sidebarMenuItem: ManifestSidebarMenuItem = {
	type: 'sidebarMenuItem',
	alias: 'Umb.SidebarMenuItem.Documents',
	name: 'Documents Sidebar Menu Item',
	weight: 100,
	loader: () => import('./document-sidebar-menu-item.element'),
	meta: {
		label: 'Documents',
		icon: 'umb:folder',
		sidebarMenus: ['Umb.SidebarMenu.Content'],
	},
};

export const manifests = [sidebarMenuItem];
