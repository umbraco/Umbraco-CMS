import type { UMB_WORKSPACE_ENTITY_IS_NEW_CONDITION_ALIAS } from './constants.js';
import type { UmbConditionConfigBase } from '@umbraco-cms/backoffice/extension-api';

export interface UmbWorkspaceEntityIsNewConditionConfig
	extends UmbConditionConfigBase<typeof UMB_WORKSPACE_ENTITY_IS_NEW_CONDITION_ALIAS> {
	match: boolean;
}

/**
 * @deprecated Use `UmbWorkspaceEntityIsNewConditionConfig` instead. This will be removed in Umbraco 17.
 */
// eslint-disable-next-line @typescript-eslint/no-empty-object-type, @typescript-eslint/naming-convention
export interface WorkspaceEntityIsNewConditionConfig extends UmbWorkspaceEntityIsNewConditionConfig {}

declare global {
	interface UmbExtensionConditionConfigMap {
		umbWorkspaceEntityIsNewConditionConfig: UmbWorkspaceEntityIsNewConditionConfig;
	}
}
