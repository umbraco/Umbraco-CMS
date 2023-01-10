import type { ManifestSidebarMenuItem } from '@umbraco-cms/models';

const sidebarMenuItem: ManifestSidebarMenuItem = {
	type: 'sidebarMenuItem',
	alias: 'Umb.SidebarMenuItem.DataTypes',
	name: 'Data Types Sidebar Menu Item',
	weight: 400,
	loader: () => import('./data-types-sidebar-menu-item.element'),
	meta: {
		label: 'Data Types',
		icon: 'umb:folder',
		sections: ['Umb.Section.Settings'],
		entityType: 'data-type',
	},
};

export const manifests = [sidebarMenuItem];
