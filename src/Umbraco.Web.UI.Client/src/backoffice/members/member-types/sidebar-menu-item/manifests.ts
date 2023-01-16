import type { ManifestSidebarMenuItem } from '@umbraco-cms/models';

const sidebarMenuItem: ManifestSidebarMenuItem = {
	type: 'sidebarMenuItem',
	alias: 'Umb.SidebarMenuItem.MemberTypes',
	name: 'Member Types Sidebar Menu Item',
	weight: 30,
	loader: () => import('./member-types-sidebar-menu-item.element'),
	meta: {
		label: 'Member Types',
		icon: 'umb:folder',
		sections: ['Umb.Section.Settings'],
	},
};

export const manifests = [sidebarMenuItem];
