import type { UmbConditionConfigBase } from '@umbraco-cms/backoffice/extension-api';
import type { UMB_WORKSPACE_HAS_CONTENT_COLLECTION_CONDITION_ALIAS } from './constants.js';

export type UmbWorkspaceHasContentCollectionConditionConfig = UmbConditionConfigBase<
	typeof UMB_WORKSPACE_HAS_CONTENT_COLLECTION_CONDITION_ALIAS
>;
/**
 * @deprecated Use `UmbWorkspaceHasContentCollectionConditionConfig` instead. This will be removed in Umbraco 17.
 */
export type WorkspaceHasCollectionConditionConfig = UmbWorkspaceHasContentCollectionConditionConfig;

declare global {
	interface UmbExtensionConditionConfigMap {
		umbWorkspaceHasContentCollection: UmbWorkspaceHasContentCollectionConditionConfig;
	}
}
