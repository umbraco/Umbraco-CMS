import type { ManifestSectionSidebarMenu } from '@umbraco-cms/models';

const sectionSidebarMenu: ManifestSectionSidebarMenu = {
	type: 'sectionSidebarMenu',
	alias: 'Umb.SectionSidebarMenu.Templating',
	name: 'Templating Section Sidebar Menu',
	weight: 100,
	meta: {
		label: 'Templating',
		sections: ['Umb.Section.Settings'],
		menu: 'Umb.Menu.Templating',
	},
};

export const manifests = [sectionSidebarMenu];
