import { UMB_ENTITY_UNIQUE_CONDITION_ALIAS } from './constants.js';
import { UmbEntityUniqueCondition } from './entity-unique.condition.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'condition',
		name: 'Umbraco Entity Unique Condition',
		alias: UMB_ENTITY_UNIQUE_CONDITION_ALIAS,
		api: UmbEntityUniqueCondition,
	},
];
