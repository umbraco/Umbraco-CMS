import { UMB_USER_GROUPS_VALUE_TYPE } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'valueSummary',
		alias: 'Umb.ValueSummary.User.UserGroups',
		name: 'User Groups Value Summary',
		forValueType: UMB_USER_GROUPS_VALUE_TYPE,
		element: () => import('./user-group-value-summary.element.js'),
		api: () => import('./user-group-value-summary.api.js'),
	},
];
