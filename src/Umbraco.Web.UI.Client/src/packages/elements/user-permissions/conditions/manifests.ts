import {
	UMB_ELEMENT_OR_ELEMENT_FOLDER_USER_PERMISSION_CONDITION_ALIAS,
	UMB_ELEMENT_USER_PERMISSION_CONDITION_ALIAS,
} from './constants.js';
import { UmbElementOrElementFolderUserPermissionCondition } from './element-or-element-folder-user-permission.condition.js';
import { UmbElementUserPermissionCondition } from './element-user-permission.condition.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'condition',
		name: 'Element User Permission Condition',
		alias: UMB_ELEMENT_USER_PERMISSION_CONDITION_ALIAS,
		api: UmbElementUserPermissionCondition,
	},
	{
		type: 'condition',
		name: 'Element or Element Folder User Permission Condition',
		alias: UMB_ELEMENT_OR_ELEMENT_FOLDER_USER_PERMISSION_CONDITION_ALIAS,
		api: UmbElementOrElementFolderUserPermissionCondition,
	},
];
