import { UMB_SECTION_ALIAS_CONDITION_ALIAS } from '@umbraco-cms/backoffice/section';
import { UMB_SETTINGS_SECTION_ALIAS } from '@umbraco-cms/backoffice/settings';

export const manifests = [
	{
		type: 'dashboard',
		alias: 'Umb.Dashboard.Telemetry',
		name: 'Telemetry',
		element: () => import('./dashboard-telemetry.element.js'),
		weight: 100,
		meta: {
			label: 'Telemetry Data',
			pathname: 'telemetry',
		},
		conditions: [
			{
				alias: UMB_SECTION_ALIAS_CONDITION_ALIAS,
				match: UMB_SETTINGS_SECTION_ALIAS,
			},
		],
	},
];

export const name = 'Umbraco.Core.Telemetry';
export const extensions = [
	{
		name: 'Telemetry Bundle',
		alias: 'Umb.Bundle.Telemetry',
		type: 'bundle',
		js: {
			manifests,
		},
	},
];
