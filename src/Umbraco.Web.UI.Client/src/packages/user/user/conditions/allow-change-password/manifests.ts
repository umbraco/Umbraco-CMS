import { UMB_USER_ALLOW_CHANGE_PASSWORD_CONDITION_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'condition',
		name: 'User Allow Change Password Condition',
		alias: UMB_USER_ALLOW_CHANGE_PASSWORD_CONDITION_ALIAS,
		api: () => import('./user-allow-change-password-action.condition.js'),
	},
];
