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
		sections: ['Umb.Section.Members'],
		entityType: 'member',
	},
};

export const manifests = [sidebarMenuItem];
