import type { ManifestSection, ManifestMenuSectionSidebarApp } from '@umbraco-cms/models';

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

const menuSectionSidebarApp: ManifestMenuSectionSidebarApp = {
	type: 'menuSectionSidebarApp',
	alias: 'Umb.SectionSidebarMenu.Settings',
	name: 'Settings Section Sidebar Menu',
	weight: 100,
	meta: {
		label: 'Settings',
		sections: [sectionAlias],
		menu: 'Umb.Menu.Settings',
	},
};

export const manifests = [section, menuSectionSidebarApp];
