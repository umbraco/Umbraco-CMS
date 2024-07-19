import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<ManifestTypes> = [
	{
		type: 'dashboard',
		alias: 'Umb.Dashboard.ModelsBuilder',
		name: 'Models Builder Dashboard',
		element: () => import('./models-builder-dashboard.element.js'),
		weight: 200,
		meta: {
			label: '#dashboardTabs_settingsModelsBuilder',
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
