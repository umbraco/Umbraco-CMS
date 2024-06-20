import type { ManifestDashboard, ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

const dashboards: Array<ManifestDashboard> = [
	{
		type: 'dashboard',
		alias: 'Umb.Dashboard.RedirectManagement',
		name: 'Redirect Management Dashboard',
		element: () => import('./dashboard-redirect-management.element.js'),
		weight: 10,
		meta: {
			label: '#dashboardTabs_contentRedirectManager',
			pathname: 'redirect-management',
		},
		conditions: [
			{
				alias: 'Umb.Condition.SectionAlias',
				match: 'Umb.Section.Content',
			},
		],
	},
];

export const manifests: Array<ManifestTypes> = [...dashboards];
