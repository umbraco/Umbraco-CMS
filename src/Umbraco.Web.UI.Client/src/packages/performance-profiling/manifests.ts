import { UMB_SECTION_ALIAS_CONDITION_ALIAS } from '@umbraco-cms/backoffice/section';
import { UMB_SETTINGS_SECTION_ALIAS } from '@umbraco-cms/backoffice/settings';

export const manifests = [
	{
		type: 'dashboard',
		alias: 'Umb.Dashboard.Profiling',
		name: 'Profiling',
		element: () => import('./dashboard-performance-profiling.element.js'),
		weight: 101,
		meta: {
			label: '#dashboardTabs_settingsProfiler',
			pathname: 'profiling',
		},
		conditions: [
			{
				alias: UMB_SECTION_ALIAS_CONDITION_ALIAS,
				match: UMB_SETTINGS_SECTION_ALIAS,
			},
		],
	},
];
