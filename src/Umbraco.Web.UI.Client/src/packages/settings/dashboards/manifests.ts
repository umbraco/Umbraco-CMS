import type { ManifestDashboard, ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

const sectionAlias = 'Umb.Section.Settings';

const dashboards: Array<ManifestDashboard> = [
	{
		type: 'dashboard',
		alias: 'Umb.Dashboard.PublishedStatus',
		name: 'Published Status Dashboard',
		element: () => import('./published-status/dashboard-published-status.element.js'),
		weight: 300,
		meta: {
			label: '#dashboardTabs_settingsPublishedStatus',
			pathname: 'published-status',
		},
		conditions: [
			{
				alias: 'Umb.Condition.SectionAlias',
				match: sectionAlias,
			},
		],
	},
];

export const manifests: Array<ManifestTypes> = [...dashboards];
