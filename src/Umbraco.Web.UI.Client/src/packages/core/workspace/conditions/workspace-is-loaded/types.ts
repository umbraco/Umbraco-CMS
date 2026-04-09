import type { UMB_CONTENT_WORKSPACE_IS_LOADED_CONDITION_ALIAS } from './constants.js';
import type { UmbConditionConfigBase } from '@umbraco-cms/backoffice/extension-api';

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface UmbContentWorkspaceIsLoadedConditionConfig
	extends UmbConditionConfigBase<typeof UMB_CONTENT_WORKSPACE_IS_LOADED_CONDITION_ALIAS> {}

declare global {
	interface UmbExtensionConditionConfigMap {
		umbContentWorkspaceIsLoadedConditionConfig: UmbContentWorkspaceIsLoadedConditionConfig;
	}
}
