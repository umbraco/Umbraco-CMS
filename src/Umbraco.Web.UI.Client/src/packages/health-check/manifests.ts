export const manifests = [
	{
		type: 'dashboard',
		alias: 'Umb.Dashboard.HealthCheck',
		name: 'Health Check',
		elementName: 'umb-dashboard-health-check',
		loader: () => import('./dashboard-health-check.element.js'),
		weight: 102,
		meta: {
			label: 'Health Check',
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
