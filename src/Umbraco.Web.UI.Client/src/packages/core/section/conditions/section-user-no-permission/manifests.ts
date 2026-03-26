import { UmbSectionUserNoPermissionCondition } from './section-user-no-permission.condition.js';
import { UMB_SECTION_USER_NO_PERMISSION_CONDITION_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'condition',
		name: 'Section User No Permission Condition',
		alias: UMB_SECTION_USER_NO_PERMISSION_CONDITION_ALIAS,
		api: UmbSectionUserNoPermissionCondition,
	},
];
