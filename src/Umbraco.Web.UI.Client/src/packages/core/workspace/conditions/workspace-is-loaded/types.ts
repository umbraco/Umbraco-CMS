import type { UMB_ENTITY_DETAIL_WORKSPACE_IS_LOADED_CONDITION_ALIAS } from './constants.js';
import type { UmbConditionConfigBase } from '@umbraco-cms/backoffice/extension-api';

export interface UmbEntityDetailWorkspaceIsLoadedConditionConfig
	extends UmbConditionConfigBase<typeof UMB_ENTITY_DETAIL_WORKSPACE_IS_LOADED_CONDITION_ALIAS> {}

declare global {
	interface UmbExtensionConditionConfigMap {
		umbEntityDetailWorkspaceIsLoadedConditionConfig: UmbEntityDetailWorkspaceIsLoadedConditionConfig;
	}
}
