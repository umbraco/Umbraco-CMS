//import { UMB_USER_MANAGEMENT_DASHBOARD_ALIAS } from '../../../dashboard/index.js';
//import { UMB_DASHBOARD_ALIAS_CONDITION_ALIAS } from '@umbraco-cms/backoffice/dashboard';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'dashboardApp',
		alias: 'Umb.DashboardApp.User.PendingInvites',
		name: 'Pending User Invites Dashboard App',
		weight: 300,
		element: () => import('./pending-user-invites-dashboard-app.element.js'),
		meta: {
			headline: 'Pending Invites',
			size: 'medium',
		},
		/*
		conditions: [
			{
				alias: UMB_DASHBOARD_ALIAS_CONDITION_ALIAS,
				match: UMB_USER_MANAGEMENT_DASHBOARD_ALIAS,
			},
		],
		*/
	},
];
