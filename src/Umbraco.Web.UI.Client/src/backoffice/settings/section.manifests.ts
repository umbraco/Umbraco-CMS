import type { ManifestSection, ManifestSidebarMenu } from '@umbraco-cms/models';

const sectionAlias = 'Umb.Section.Settings';

const section: ManifestSection = {
	type: 'section',
	alias: sectionAlias,
	name: 'Settings Section',
	weight: 300,
	meta: {
		label: 'Settings',
		pathname: 'settings',
	},
};

const sidebarMenu: ManifestSidebarMenu = {
	type: 'sidebarMenu',
	alias: 'Umb.SidebarMenu.Settings',
	name: 'Settings Sidebar Menu',
	weight: 100,
	meta: {
		label: 'Settings',
		sections: [sectionAlias],
	},
};

export const manifests = [section, sidebarMenu];
