import { UMB_ELEMENT_FOLDER_USER_PERMISSION_CONDITION_ALIAS } from './constants.js';
import { UmbElementFolderUserPermissionCondition } from './element-folder-user-permission.condition.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'condition',
		name: 'Element Folder User Permission Condition',
		alias: UMB_ELEMENT_FOLDER_USER_PERMISSION_CONDITION_ALIAS,
		api: UmbElementFolderUserPermissionCondition,
	},
];
