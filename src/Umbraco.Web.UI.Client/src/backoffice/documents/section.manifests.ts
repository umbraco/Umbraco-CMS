import type { ManifestSection, ManifestSectionSidebarMenu } from '@umbraco-cms/models';

const sectionAlias = 'Umb.Section.Content';

const section: ManifestSection = {
	type: 'section',
	alias: sectionAlias,
	name: 'Content Section',
	weight: 600,
	meta: {
		label: 'Content',
		pathname: 'content',
	},
};

const sectionSidebarMenu: ManifestSectionSidebarMenu = {
	type: 'sectionSidebarMenu',
	alias: 'Umb.SidebarMenu.Content',
	name: 'Content Sidebar Menu',
	weight: 100,
	meta: {
		label: 'Content',
		sections: [sectionAlias],
		menu: 'Umb.Menu.Content',
	},
};

export const manifests = [section, sectionSidebarMenu];
