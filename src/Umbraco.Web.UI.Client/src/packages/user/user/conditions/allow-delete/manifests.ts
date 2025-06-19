import { UMB_USER_ALLOW_DELETE_CONDITION_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'condition',
		name: 'User Allow Delete Action Condition',
		alias: UMB_USER_ALLOW_DELETE_CONDITION_ALIAS,
		api: () => import('./user-allow-delete-action.condition.js'),
	},
];
