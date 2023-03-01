import type { ManifestDashboardCollection, ManifestSection, ManifestMenuSectionSidebarApp } from '@umbraco-cms/models';

const sectionAlias = 'Umb.Section.Media';

const section: ManifestSection = {
	type: 'section',
	alias: sectionAlias,
	name: 'Media Section',
	weight: 500,
	meta: {
		label: 'Media',
		pathname: 'media',
	},
};

const dashboards: Array<ManifestDashboardCollection> = [
	{
		type: 'dashboardCollection',
		alias: 'Umb.Dashboard.MediaCollection',
		name: 'Media Dashboard',
		weight: 10,
		meta: {
			label: 'Media',
			sections: [sectionAlias],
			pathname: 'media-management',
			entityType: 'media',
			repositoryAlias: 'Umb.Repository.Media',
		},
	},
];

const menuSectionSidebarApp: ManifestMenuSectionSidebarApp = {
	type: 'menuSectionSidebarApp',
	alias: 'Umb.SectionSidebarMenu.Media',
	name: 'Media Section Sidebar Menu',
	weight: 100,
	meta: {
		label: 'Media',
		sections: [sectionAlias],
		menu: 'Umb.Menu.Media',
	},
};

export const manifests = [section, menuSectionSidebarApp, ...dashboards];
