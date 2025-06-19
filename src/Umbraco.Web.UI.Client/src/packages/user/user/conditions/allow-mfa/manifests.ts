import { UMB_USER_ALLOW_MFA_CONDITION_ALIAS, UMB_CURRENT_USER_ALLOW_MFA_CONDITION_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'condition',
		name: 'User Allow Mfa Action Condition',
		alias: UMB_USER_ALLOW_MFA_CONDITION_ALIAS,
		api: () => import('./user-allow-mfa-action.condition.js'),
	},
	{
		type: 'condition',
		name: 'Current User Allow Mfa Action Condition',
		alias: UMB_CURRENT_USER_ALLOW_MFA_CONDITION_ALIAS,
		api: () => import('./current-user-allow-mfa-action.condition.js'),
	},
];
