import { UMB_USER_IS_DEFAULT_KIND_CONDITION_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'condition',
		name: 'User Is Default Kind Condition',
		alias: UMB_USER_IS_DEFAULT_KIND_CONDITION_ALIAS,
		api: () => import('./user-is-default-kind.condition.js'),
	},
];
