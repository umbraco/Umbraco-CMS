import type { UMB_WORKSPACE_ENTITY_IS_NEW_CONDITION_ALIAS } from './constants.js';
import type { UmbConditionConfigBase } from '@umbraco-cms/backoffice/extension-api';

export interface UmbWorkspaceEntityIsNewConditionConfig
	extends UmbConditionConfigBase<typeof UMB_WORKSPACE_ENTITY_IS_NEW_CONDITION_ALIAS> {
	match: boolean;
}

declare global {
	interface UmbExtensionConditionConfigMap {
		umbWorkspaceEntityIsNewConditionConfig: UmbWorkspaceEntityIsNewConditionConfig;
	}
}
