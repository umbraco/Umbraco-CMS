import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<ManifestTypes> = [
	{
		type: 'dashboard',
		alias: 'Umb.Dashboard.HealthCheck',
		name: 'Health Check',
		elementName: 'umb-dashboard-health-check',
		js: () => import('./dashboard-health-check.element.js'),
		weight: 102,
		meta: {
			label: '#dashboardTabs_settingsHealthCheck',
			pathname: 'health-check',
		},
		conditions: [
			{
				alias: 'Umb.Condition.SectionAlias',
				match: 'Umb.Section.Settings',
			},
		],
	},
];
