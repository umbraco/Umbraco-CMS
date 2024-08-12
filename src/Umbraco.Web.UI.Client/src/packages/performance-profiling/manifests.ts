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
				alias: 'Umb.Condition.SectionAlias',
				match: 'Umb.Section.Settings',
			},
		],
	},
];
