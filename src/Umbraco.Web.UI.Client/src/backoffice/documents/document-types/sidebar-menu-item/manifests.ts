import type { ManifestSidebarMenuItem } from '@umbraco-cms/models';

const sidebarMenuItem: ManifestSidebarMenuItem = {
	type: 'sidebarMenuItem',
	alias: 'Umb.SidebarMenuItem.DocumentTypes',
	name: 'Document Types Sidebar Menu Item',
	weight: 400,
	loader: () => import('./document-types-sidebar-menu-item.element'),
	meta: {
		label: 'Document Types',
		icon: 'umb:folder',
		sections: ['Umb.Section.Settings'],
	},
};

export const manifests = [sidebarMenuItem];
