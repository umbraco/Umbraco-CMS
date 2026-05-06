import { UMB_USER_STATE_VALUE_TYPE } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'valueSummary',
		kind: 'default',
		alias: 'Umb.ValueSummary.User.State',
		name: 'User State Value Summary',
		forValueType: UMB_USER_STATE_VALUE_TYPE,
		element: () => import('./state/user-state-value-summary.element.js'),
	},
];
