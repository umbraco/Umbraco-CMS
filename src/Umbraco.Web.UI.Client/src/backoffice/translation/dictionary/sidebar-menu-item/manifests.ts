import type { ManifestSidebarMenuItem } from '@umbraco-cms/models';

const sidebarMenuItem: ManifestSidebarMenuItem = {
	type: 'sidebarMenuItem',
	alias: 'Umb.SidebarMenuItem.Dictionary',
	name: 'Dictionary Sidebar Menu Item',
	weight: 400,
	loader: () => import('./dictionary-sidebar-menu-item.element'),
	meta: {
		label: 'Dictionary',
		icon: 'umb:folder',
		sections: ['Umb.Section.Translation'],
	},
};

export const manifests = [sidebarMenuItem];
