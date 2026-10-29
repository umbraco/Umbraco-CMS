import type { UMB_WORKSPACE_ENTITY_IS_READ_ONLY_CONDITION_ALIAS } from './constants.js';
import type { UmbConditionConfigBase } from '@umbraco-cms/backoffice/extension-api';

/**
 * Configuration for the condition that checks whether the entity of an entity detail workspace is read-only, as the
 * workspace's `readOnlyGuard` reports it through `permitted`. In a workspace with variants, a rule for a single variant
 * counts as well; use the guard's `isPermittedForVariant` to tell variants apart.
 * Outside an entity detail workspace the condition is never met, whatever `match` is.
 */
export interface UmbWorkspaceEntityIsReadOnlyConditionConfig extends UmbConditionConfigBase<
	typeof UMB_WORKSPACE_ENTITY_IS_READ_ONLY_CONDITION_ALIAS
> {
	/**
	 * Whether the workspace entity must be read-only for the condition to be met. Defaults to true.
	 */
	match?: boolean;
}

declare global {
	interface UmbExtensionConditionConfigMap {
		umbWorkspaceEntityIsReadOnlyConditionConfig: UmbWorkspaceEntityIsReadOnlyConditionConfig;
	}
}
