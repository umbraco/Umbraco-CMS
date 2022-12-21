import type { ManifestDashboard, ManifestSection } from '@umbraco-cms/models';

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

const dashboards: Array<ManifestDashboard> = [
	{
		type: 'dashboard',
		alias: 'Umb.Dashboard.MediaManagement',
		name: 'Media Dashboard',
		loader: () => import('../../dashboards/media-management/dashboard-media-management.element'),
		weight: 10,
		meta: {
			label: 'Media',
			sections: [sectionAlias],
			pathname: 'media-management',
		},
	},
];

export const manifests = [section, ...dashboards];
