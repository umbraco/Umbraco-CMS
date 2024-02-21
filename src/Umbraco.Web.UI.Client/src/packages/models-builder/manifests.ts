export const manifests = [
	{
		type: 'dashboard',
		alias: 'Umb.Dashboard.ModelsBuilder',
		name: 'Models Builder Dashboard',
		element: () => import('./models-builder-dashboard.element.js'),
		weight: 300,
		meta: {
			label: 'Models Builder',
			pathname: 'models-builder',
		},
		conditions: [
			{
				alias: 'Umb.Condition.SectionAlias',
				match: 'Umb.Section.Settings',
			},
		],
	},
];
