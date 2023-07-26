import type { ManifestDashboard } from '@umbraco-cms/backoffice/extension-registry';

const dashboards: Array<ManifestDashboard> = [
	{
		type: 'dashboard',
		alias: 'Umb.Dashboard.RedirectManagement',
		name: 'Redirect Management Dashboard',
		loader: () => import('./redirect-management/dashboard-redirect-management.element.js'),
		weight: 10,
		meta: {
			label: 'Redirect Management',
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

export const manifests = [...dashboards];
