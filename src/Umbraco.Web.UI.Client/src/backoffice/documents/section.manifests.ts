import type { ManifestSection, ManifestSidebarMenu } from '@umbraco-cms/models';

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

const sidebarMenu: ManifestSidebarMenu = {
	type: 'sidebarMenu',
	alias: 'Umb.SidebarMenu.Content',
	name: 'Content Sidebar Menu',
	weight: 100,
	meta: {
		label: 'Content',
		sections: [sectionAlias],
	},
};

export const manifests = [section, sidebarMenu];
