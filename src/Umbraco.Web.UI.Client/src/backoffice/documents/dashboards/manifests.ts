import type { ManifestDashboard } from '@umbraco-cms/models';

const dashboards: Array<ManifestDashboard> = [
	{
		type: 'dashboard',
		alias: 'Umb.Dashboard.RedirectManagement',
		name: 'Redirect Management Dashboard',
		loader: () => import('./redirect-management/dashboard-redirect-management.element'),
		weight: 10,
		meta: {
			label: 'Redirect Management',
			pathname: 'redirect-management',
		},
		conditions: {
			sections: ['Umb.Section.Content'],
		},
	},
	{
		type: 'dashboard',
		alias: 'Umb.Dashboard.Welcome',
		name: 'Welcome Dashboard',
		loader: () => import('../../documents/dashboards/welcome/dashboard-welcome.element'),
		weight: 20,
		meta: {
			label: 'Welcome',
			pathname: 'welcome',
		},
		conditions: {
			sections: ['Umb.Section.Content'],
		},
	},
];

export const manifests = [...dashboards];
