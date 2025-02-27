import { manifests as userPermissionManifests } from './user-permission/manifests.js';
import { UMB_SETTINGS_SECTION_ALIAS } from '@umbraco-cms/backoffice/settings';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'dashboard',
		alias: 'Umb.Dashboard.HealthCheck',
		name: 'Health Check',
		elementName: 'umb-dashboard-health-check',
		element: () => import('./dashboard-health-check.element.js'),
		weight: 102,
		meta: {
			label: '#dashboardTabs_settingsHealthCheck',
			pathname: 'health-check',
		},
		conditions: [
			{
				alias: 'Umb.Condition.SectionAlias',
				match: UMB_SETTINGS_SECTION_ALIAS,
			},
		],
	},
	...userPermissionManifests,
];
