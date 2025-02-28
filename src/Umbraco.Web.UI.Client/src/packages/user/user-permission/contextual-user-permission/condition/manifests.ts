import { UMB_CONTEXTUAL_USER_PERMISSION_CONDITION_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'condition',
		name: 'Contextual User Permission Condition',
		alias: UMB_CONTEXTUAL_USER_PERMISSION_CONDITION_ALIAS,
		api: () => import('./contextual-user-permission.condition.js'),
	},
];
