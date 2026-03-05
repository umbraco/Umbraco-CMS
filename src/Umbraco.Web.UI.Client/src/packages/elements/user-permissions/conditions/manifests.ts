import { UMB_ELEMENT_USER_PERMISSION_CONDITION_ALIAS } from './constants.js';
import { UmbElementUserPermissionCondition } from './element-user-permission.condition.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'condition',
		name: 'Element User Permission Condition',
		alias: UMB_ELEMENT_USER_PERMISSION_CONDITION_ALIAS,
		api: UmbElementUserPermissionCondition,
	},
];
