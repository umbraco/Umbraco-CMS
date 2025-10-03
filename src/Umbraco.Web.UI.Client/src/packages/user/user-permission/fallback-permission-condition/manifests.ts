import { UMB_FALLBACK_USER_PERMISSION_CONDITION_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'condition',
		name: 'User Fallback Permission Condition',
		alias: UMB_FALLBACK_USER_PERMISSION_CONDITION_ALIAS,
		api: () => import('./fallback-user-permission.condition.js'),
	},
];
