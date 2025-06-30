import { UmbSectionUserPermissionCondition } from './section-user-permission.condition.js';
import { UMB_SECTION_USER_PERMISSION_CONDITION_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'condition',
		name: 'Section User Permission Condition',
		alias: UMB_SECTION_USER_PERMISSION_CONDITION_ALIAS,
		api: UmbSectionUserPermissionCondition,
	},
];
