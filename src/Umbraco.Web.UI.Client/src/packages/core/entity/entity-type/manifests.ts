import { UMB_ENTITY_TYPE_CONDITION_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'condition',
		name: 'Umbraco Entity Unique Condition',
		alias: UMB_ENTITY_TYPE_CONDITION_ALIAS,
		api: () => import('./entity-type.condition.js'),
	},
];
