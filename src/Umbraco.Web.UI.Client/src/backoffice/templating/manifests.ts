import type { ManifestSidebarMenu } from '@umbraco-cms/models';

const sidebarMenu: ManifestSidebarMenu = {
	type: 'sidebarMenu',
	alias: 'Umb.SidebarMenu.Templating',
	name: 'Settings Sidebar Menu',
	weight: 100,
	meta: {
		label: 'Templating',
		sections: ['Umb.Section.Settings'],
	},
};

export const manifests = [sidebarMenu];
