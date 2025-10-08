export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'condition',
		name: 'Dashboard Alias Condition',
		alias: 'Umb.Condition.DashboardAlias',
		api: () => import('./dashboard-alias.condition.js'),
	},
];
