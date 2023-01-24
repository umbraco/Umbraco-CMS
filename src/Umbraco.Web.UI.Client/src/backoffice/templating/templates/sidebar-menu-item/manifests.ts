import type { ManifestSidebarMenuItem } from '@umbraco-cms/models';

const sidebarMenuItem: ManifestSidebarMenuItem = {
	type: 'sidebarMenuItem',
	alias: 'Umb.SidebarMenuItem.Templates',
	name: 'Templates Sidebar Menu Item',
	weight: 40,
	loader: () => import('./templates-sidebar-menu-item.element'),
	meta: {
		label: 'Templates',
		icon: 'umb:folder',
		sections: ['Umb.Section.Settings'],
		entityType: 'template',
	},
};

export const manifests = [sidebarMenuItem];
