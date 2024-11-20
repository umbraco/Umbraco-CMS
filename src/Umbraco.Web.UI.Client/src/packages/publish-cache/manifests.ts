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
				alias: 'Umb.Condition.SectionAlias',
				match: 'Umb.Section.Settings',
			},
		],
	},
];
