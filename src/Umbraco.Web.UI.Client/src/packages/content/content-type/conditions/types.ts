import type { UMB_WORKSPACE_CONTENT_TYPE_ALIAS_CONDITION_ALIAS } from './constants.js';
import type { UmbConditionConfigBase } from '@umbraco-cms/backoffice/extension-api';

export type UmbWorkspaceContentTypeAliasConditionConfig = UmbConditionConfigBase<
	typeof UMB_WORKSPACE_CONTENT_TYPE_ALIAS_CONDITION_ALIAS
> & {
	/**
	 * Define a content type alias in which workspace this extension should be available
	 * @example
	 * Depends on implementation, but i.e. "article", "image", "blockPage"
	 */
	match?: string;
	/**
	 * Define one or more content type aliases in which workspace this extension should be available
	 * @example
	 * ["article", "image", "blockPage"]
	 */
	oneOf?: Array<string>;
};
/**
 * @deprecated Use `UmbWorkspaceContentTypeAliasConditionConfig` instead. This will be removed in Umbraco 17.
 */
export type WorkspaceContentTypeAliasConditionConfig = UmbWorkspaceContentTypeAliasConditionConfig;

declare global {
	interface UmbExtensionConditionConfigMap {
		umbWorkspaceContentTypeAliasConditionConfig: UmbWorkspaceContentTypeAliasConditionConfig;
	}
}
