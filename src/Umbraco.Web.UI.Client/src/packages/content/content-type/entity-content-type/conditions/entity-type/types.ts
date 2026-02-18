import type { UMB_ENTITY_CONTENT_TYPE_ENTITY_TYPE_CONDITION_ALIAS } from './constants.js';
import type { UmbConditionConfigBase } from '@umbraco-cms/backoffice/extension-api';

export type UmbEntityContentTypeEntityTypeConditionConfig = UmbConditionConfigBase<
	typeof UMB_ENTITY_CONTENT_TYPE_ENTITY_TYPE_CONDITION_ALIAS
> & {
	/**
	 * Define the entity type of the entity content type for which this extension should be available.
	 * @example
	 * "document-type"
	 */
	match?: string;
	/**
	 * Define one or more entity types of entity content types for which this extension should be available.
	 * @example
	 * ["document-type", "media-type"]
	 */
	oneOf?: Array<string>;
};

declare global {
	interface UmbExtensionConditionConfigMap {
		umbEntityContentTypeEntityTypeConditionConfig: UmbEntityContentTypeEntityTypeConditionConfig;
	}
}
