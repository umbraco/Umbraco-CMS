import type { UMB_ENTITY_CONTENT_TYPE_UNIQUE_CONDITION_ALIAS } from './constants.js';
import type { UmbConditionConfigBase } from '@umbraco-cms/backoffice/extension-api';

export type UmbEntityContentTypeUniqueConditionConfig = UmbConditionConfigBase<
	typeof UMB_ENTITY_CONTENT_TYPE_UNIQUE_CONDITION_ALIAS
> & {
	/**
	 * Define the unique (GUID) of the entity content type for which this extension should be available.
	 * @example
	 * "d59be02f-1df9-4228-aa1e-01917d806cda"
	 */
	match?: string;
	/**
	 * Define one or more unique (GUIDs) of entity content types for which this extension should be available.
	 * @example
	 * ["d59be02f-1df9-4228-aa1e-01917d806cda", "42d7572e-1ba1-458d-a765-95b60040c3ac"]
	 */
	oneOf?: Array<string>;
};

declare global {
	interface UmbExtensionConditionConfigMap {
		umbEntityContentTypeUniqueConditionConfig: UmbEntityContentTypeUniqueConditionConfig;
	}
}
