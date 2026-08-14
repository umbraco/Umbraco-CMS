import { UMB_WRITABLE_PROPERTY_CONDITION_ALIAS } from './constants.js';
import { UmbWritablePropertyCondition } from './writable-property.condition.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'condition',
		name: 'Writable Property Condition',
		alias: UMB_WRITABLE_PROPERTY_CONDITION_ALIAS,
		api: UmbWritablePropertyCondition,
	},
];
