export const manifests = [
	{
		type: 'entityAction',
		kind: 'default',
		alias: 'Umb.EntityAction.DashboardApp.Remove',
		name: 'Remove Dashboard App',
		forEntityTypes: ['dashboardApp'],
		api: () => import('./remove-dashboard-app.entity-action.js'),
		meta: {
			label: '#actions_remove',
			icon: 'icon-trash',
			weight: 100,
		},
	},
];
