import { UMB_PROPERTY_HAS_VALUE_CONDITION_ALIAS } from './constants.js';
import { UmbPropertyHasValueCondition } from './has-value.condition.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'condition',
		name: 'Property Has Value Condition',
		alias: UMB_PROPERTY_HAS_VALUE_CONDITION_ALIAS,
		api: UmbPropertyHasValueCondition,
	},
];
