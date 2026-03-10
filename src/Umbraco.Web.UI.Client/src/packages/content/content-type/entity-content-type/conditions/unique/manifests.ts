import { UMB_ENTITY_CONTENT_TYPE_UNIQUE_CONDITION_ALIAS } from './constants.js';
import { UmbEntityContentTypeUniqueCondition } from './entity-content-type-unique.condition.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'condition',
		name: 'Entity Content Type Unique Condition',
		alias: UMB_ENTITY_CONTENT_TYPE_UNIQUE_CONDITION_ALIAS,
		api: UmbEntityContentTypeUniqueCondition,
	},
];
