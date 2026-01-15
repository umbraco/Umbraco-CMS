import type { UMB_WORKSPACE_ENTITY_TYPE_CONDITION_ALIAS } from './constants.js';
import type { UmbConditionConfigBase } from '@umbraco-cms/backoffice/extension-api';

export type UmbWorkspaceEntityTypeConditionConfig = UmbConditionConfigBase<
	typeof UMB_WORKSPACE_ENTITY_TYPE_CONDITION_ALIAS
> & {
	/**
	 * Define the workspace that this extension should be available in
	 * @example
	 * "Document"
	 */
	match: string;
};

declare global {
	interface UmbExtensionConditionConfigMap {
		umbWorkspaceEntityType: UmbWorkspaceEntityTypeConditionConfig;
	}
}
