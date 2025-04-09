import type { UmbConditionConfigBase } from '@umbraco-cms/backoffice/extension-api';
import type { UMB_WORKSPACE_HAS_COLLECTION_CONDITION_ALIAS } from './constants.js';

export type UmbWorkspaceHasCollectionConditionConfig = UmbConditionConfigBase<
	typeof UMB_WORKSPACE_HAS_COLLECTION_CONDITION_ALIAS
>;
/**
 * @deprecated Use `UmbWorkspaceHasCollectionConditionConfig` instead. This will be removed in Umbraco 17.
 */
export type WorkspaceHasCollectionConditionConfig = UmbWorkspaceHasCollectionConditionConfig;

declare global {
	interface UmbExtensionConditionConfigMap {
		umbWorkspaceHasCollection: UmbWorkspaceHasCollectionConditionConfig;
	}
}
