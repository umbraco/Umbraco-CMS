export const manifests: Array<UmbExtensionManifest> = [
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
