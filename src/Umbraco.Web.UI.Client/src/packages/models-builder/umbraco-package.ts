import { UMB_SECTION_ALIAS_CONDITION_ALIAS } from '@umbraco-cms/backoffice/section';
import { UMB_SETTINGS_SECTION_ALIAS } from '@umbraco-cms/backoffice/settings';

export const manifests: Array<UmbExtensionManifest> = [
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
				alias: UMB_SECTION_ALIAS_CONDITION_ALIAS,
				match: UMB_SETTINGS_SECTION_ALIAS,
			},
		],
	},
];

export const name = 'Umbraco.Core.ModelsBuilder';
export const extensions = [
	{
		name: 'Models Builder Management Bundle',
		alias: 'Umb.Bundle.ModelsBuilder',
		type: 'bundle',
		js: {
			manifests,
		},
	},
];
