import type { ManifestDashboard } from '@umbraco-cms/models';

const dashboards: Array<ManifestDashboard> = [
	{
		type: 'dashboard',
		alias: 'Umb.Dashboard.Welcome',
		name: 'Welcome Dashboard',
		loader: () => import('./welcome/dashboard-welcome.element'),
		weight: 20,
		meta: {
			label: 'Welcome',
			sections: ['Umb.Section.Content'],
			pathname: 'welcome',
		},
	},
	{
		type: 'dashboard',
		alias: 'Umb.Dashboard.RedirectManagement',
		name: 'Redirect Management Dashboard',
		loader: () => import('./redirect-management/dashboard-redirect-management.element'),
		weight: 10,
		meta: {
			label: 'Redirect Management',
			sections: ['Umb.Section.Content'],
			pathname: 'redirect-management',
		},
	},
];

export const manifests = [...dashboards];
