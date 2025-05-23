import { UMB_USER_ALLOW_DISABLE_CONDITION_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'condition',
		name: 'User Allow Disable Action Condition',
		alias: UMB_USER_ALLOW_DISABLE_CONDITION_ALIAS,
		api: () => import('./user-allow-disable-action.condition.js'),
	},
];
