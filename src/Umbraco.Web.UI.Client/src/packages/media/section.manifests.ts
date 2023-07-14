import { MEDIA_REPOSITORY_ALIAS } from './media/repository/manifests.js';
import type {
	ManifestDashboardCollection,
	ManifestSection,
	ManifestTypes,
} from '@umbraco-cms/backoffice/extension-registry';

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
	conditions: [],
};

const dashboards: Array<ManifestDashboardCollection> = [
	{
		type: 'dashboardCollection',
		alias: 'Umb.Dashboard.MediaCollection',
		name: 'Media Dashboard',
		weight: 10,
		meta: {
			label: 'Media',
			pathname: 'media-management',
			repositoryAlias: MEDIA_REPOSITORY_ALIAS,
		},
		conditions: {
			sections: [sectionAlias],
			entityType: 'media',
		},
	},
];

const menuSectionSidebarApp: ManifestTypes = {
	type: 'sectionSidebarApp',
	kind: 'menu',
	alias: 'Umb.SectionSidebarMenu.Media',
	name: 'Media Section Sidebar Menu',
	weight: 100,
	meta: {
		label: 'Media',
		menu: 'Umb.Menu.Media',
	},
	conditions: [
		{
			alias: 'Umb.Condition.SectionAlias',
			value: sectionAlias,
		},
	],
};

export const manifests = [section, menuSectionSidebarApp, ...dashboards];
