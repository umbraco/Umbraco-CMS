import { UMB_ENTITY_CONTENT_TYPE_ENTITY_TYPE_CONDITION_ALIAS } from './constants.js';
import { UmbEntityContentTypeEntityTypeCondition } from './entity-content-type-entity-type.condition.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'condition',
		name: 'Entity Content Type Entity Type Condition',
		alias: UMB_ENTITY_CONTENT_TYPE_ENTITY_TYPE_CONDITION_ALIAS,
		api: UmbEntityContentTypeEntityTypeCondition,
	},
];
