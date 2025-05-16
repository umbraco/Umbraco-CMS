import { UMB_UI_USER_PERMISSION_CONDITION_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'condition',
		name: 'UI User Permission Condition',
		alias: UMB_UI_USER_PERMISSION_CONDITION_ALIAS,
		api: () => import('./ui-user-permission.condition.js'),
	},
];
