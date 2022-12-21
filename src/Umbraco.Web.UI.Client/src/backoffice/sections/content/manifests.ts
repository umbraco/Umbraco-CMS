import type { ManifestDashboard, ManifestSection } from '@umbraco-cms/models';

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

const dashboards: Array<ManifestDashboard> = [
	{
		type: 'dashboard',
		alias: 'Umb.Dashboard.Welcome',
		name: 'Welcome Dashboard',
		loader: () => import('../../dashboards/welcome/dashboard-welcome.element'),
		weight: 20,
		meta: {
			label: 'Welcome',
			sections: [sectionAlias],
			pathname: 'welcome',
		},
	},
	{
		type: 'dashboard',
		alias: 'Umb.Dashboard.RedirectManagement',
		name: 'Redirect Management Dashboard',
		loader: () => import('../../dashboards/redirect-management/dashboard-redirect-management.element'),
		weight: 10,
		meta: {
			label: 'Redirect Management',
			sections: [sectionAlias],
			pathname: 'redirect-management',
		},
	},
];

export const manifests = [section, ...dashboards];
