import type { ManifestDashboardCollection, ManifestSection } from '@umbraco-cms/models';

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
			storeAlias: 'umbMediaStore'
		},
	},
];

export const manifests = [section, ...dashboards];
