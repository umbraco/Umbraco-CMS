import { UMB_USER_STATE_VALUE_TYPE, UMB_USER_LAST_LOGIN_VALUE_TYPE } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'valueSummary',
		alias: 'Umb.ValueSummary.User.State',
		name: 'User State Value Summary',
		forValueType: UMB_USER_STATE_VALUE_TYPE,
		element: () => import('./state/user-state-value-summary.element.js'),
	},
	{
		type: 'valueSummary',
		alias: 'Umb.ValueSummary.User.LastLogin',
		name: 'User Last Login Value Summary',
		forValueType: UMB_USER_LAST_LOGIN_VALUE_TYPE,
		element: () => import('./last-login/user-last-login-value-summary.element.js'),
	},
];
