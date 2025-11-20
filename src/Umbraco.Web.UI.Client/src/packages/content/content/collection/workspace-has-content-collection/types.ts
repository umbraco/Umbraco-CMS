import type { UMB_WORKSPACE_HAS_CONTENT_COLLECTION_CONDITION_ALIAS } from './constants.js';
import type { UmbConditionConfigBase } from '@umbraco-cms/backoffice/extension-api';

export type UmbWorkspaceHasContentCollectionConditionConfig = UmbConditionConfigBase<
	typeof UMB_WORKSPACE_HAS_CONTENT_COLLECTION_CONDITION_ALIAS
>;

declare global {
	interface UmbExtensionConditionConfigMap {
		umbWorkspaceHasContentCollection: UmbWorkspaceHasContentCollectionConditionConfig;
	}
}
