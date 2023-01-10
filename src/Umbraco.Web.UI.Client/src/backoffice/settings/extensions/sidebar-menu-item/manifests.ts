import type { ManifestSidebarMenuItem } from '@umbraco-cms/models';

const sidebarMenuItem: ManifestSidebarMenuItem = {
	type: 'sidebarMenuItem',
	alias: 'Umb.SidebarMenuItem.Extensions',
	name: 'Extensions Sidebar Menu Item',
	weight: 400,
	meta: {
		label: 'Extensions',
		icon: 'umb:wand',
		sections: ['Umb.Section.Settings'],
		entityType: 'extension-root',
	},
};

export const manifests = [sidebarMenuItem];
