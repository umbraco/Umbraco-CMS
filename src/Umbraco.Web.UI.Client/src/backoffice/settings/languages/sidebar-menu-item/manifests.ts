import type { ManifestSidebarMenuItem } from '@umbraco-cms/models';

const sidebarMenuItem: ManifestSidebarMenuItem = {
	type: 'sidebarMenuItem',
	alias: 'Umb.SidebarMenuItem.Languages',
	name: 'Languages Sidebar Menu Item',
	weight: 80,
	meta: {
		label: 'Languages',
		icon: 'umb:globe',
		sections: ['Umb.Section.Settings'],
		entityType: 'language-root',
	},
};

export const manifests = [sidebarMenuItem];
