import { UMB_SECTION_ALIAS_CONDITION_ALIAS } from '@umbraco-cms/backoffice/section';
import { UMB_SETTINGS_SECTION_ALIAS } from '@umbraco-cms/backoffice/settings';

export const manifests = [
	{
		type: 'dashboard',
		alias: 'Umb.Dashboard.PublishedStatus',
		name: 'Published Status Dashboard',
		element: () => import('./dashboard-published-status.element.js'),
		weight: 300,
		meta: {
			label: '#dashboardTabs_settingsPublishedStatus',
			pathname: 'published-status',
		},
		conditions: [
			{
				alias: UMB_SECTION_ALIAS_CONDITION_ALIAS,
				match: UMB_SETTINGS_SECTION_ALIAS,
			},
		],
	},
];
