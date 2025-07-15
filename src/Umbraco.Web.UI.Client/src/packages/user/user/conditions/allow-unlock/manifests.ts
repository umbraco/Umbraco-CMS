import { UMB_USER_ALLOW_UNLOCK_CONDITION_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'condition',
		name: 'User Allow Unlock Action Condition',
		alias: UMB_USER_ALLOW_UNLOCK_CONDITION_ALIAS,
		api: () => import('./user-allow-unlock-action.condition.js'),
	},
];
