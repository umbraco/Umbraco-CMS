import { UMB_DATA_TYPE_ALLOW_DELETE_CONDITION_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'condition',
		name: 'Data Type Allow Delete Action Condition',
		alias: UMB_DATA_TYPE_ALLOW_DELETE_CONDITION_ALIAS,
		api: () => import('./data-type-allow-delete-action.condition.js'),
	},
];
