import type { UMB_WORKSPACE_CONTENT_TYPE_UNIQUE_CONDITION } from './constants.js';
import type { UmbConditionConfigBase } from '@umbraco-cms/backoffice/extension-api';

export type UmbWorkspaceContentTypeUniqueConditionConfig = UmbConditionConfigBase<
	typeof UMB_WORKSPACE_CONTENT_TYPE_UNIQUE_CONDITION
> & {
	/**
	 * Define a content type unique (GUID) in which workspace this extension should be available
	 * @example
	 * Depends on implementation, but i.e. "d59be02f-1df9-4228-aa1e-01917d806cda"
	 */
	match?: string;
	/**
	 * Define one or more content type unique (GUIDs) in which workspace this extension should be available
	 * @example
	 * ["d59be02f-1df9-4228-aa1e-01917d806cda", "42d7572e-1ba1-458d-a765-95b60040c3ac"]
	 */
	oneOf?: Array<string>;
};

declare global {
	interface UmbExtensionConditionConfigMap {
		umbWorkspaceContentTypeUniqueConditionConfig: UmbWorkspaceContentTypeUniqueConditionConfig;
	}
}
