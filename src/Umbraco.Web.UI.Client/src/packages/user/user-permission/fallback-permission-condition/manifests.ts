import { UMB_FALLBACK_USER_PERMISSION_CONDITION_ALIAS } from './constants.js';
import { UmbFallbackUserPermissionCondition } from './fallback-user-permission.condition.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'condition',
		name: 'User Fallback Permission Condition',
		alias: UMB_FALLBACK_USER_PERMISSION_CONDITION_ALIAS,
		api: UmbFallbackUserPermissionCondition,
	},
];
