import { UMB_USER_ALLOW_EXTERNAL_LOGIN_CONDITION_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'condition',
		name: 'User Allow ExternalLogin Action Condition',
		alias: UMB_USER_ALLOW_EXTERNAL_LOGIN_CONDITION_ALIAS,
		api: () => import('./user-allow-external-login-action.condition.js'),
	},
];
