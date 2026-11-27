import { UMB_PERMISSION_CONDITION_ALIAS } from './constants.js';
import { UmbPermissionCondition } from './permission.condition.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'condition',
		name: 'Permission Condition',
		alias: UMB_PERMISSION_CONDITION_ALIAS,
		api: UmbPermissionCondition,
	},
];
