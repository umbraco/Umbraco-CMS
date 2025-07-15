import { UMB_LANGUAGE_USER_PERMISSION_CONDITION_ALIAS } from './constants.js';

export const manifest: UmbExtensionManifest = {
	type: 'condition',
	name: 'Language User Permission Condition',
	alias: UMB_LANGUAGE_USER_PERMISSION_CONDITION_ALIAS,
	api: () => import('./language-user-permission.condition.js'),
};
