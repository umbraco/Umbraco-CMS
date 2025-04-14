import type { UMB_WORKSPACE_CONDITION_ALIAS, UMB_WORKSPACE_ENTITY_IS_NEW_CONDITION_ALIAS } from './const.js';
import type { UmbConditionConfigBase } from '@umbraco-cms/backoffice/extension-api';

export interface WorkspaceAliasConditionConfig extends UmbConditionConfigBase<typeof UMB_WORKSPACE_CONDITION_ALIAS> {
	/**
	 * Define the workspace that this extension should be available in
	 * @example
	 * "Umb.Workspace.Document"
	 */
	match?: string;
	/**
	 * Define one or more workspaces that this extension should be available in
	 * @example
	 * ["Umb.Workspace.Document", "Umb.Workspace.Media"]
	 */
	oneOf?: Array<string>;
}

export type UmbWorkspaceEntityTypeConditionConfig = UmbConditionConfigBase<'Umb.Condition.WorkspaceEntityType'> & {
	/**
	 * Define the workspace that this extension should be available in
	 * @example
	 * "Document"
	 */
	match: string;
};
/**
 * @deprecated Use `UmbWorkspaceEntityTypeConditionConfig` instead. This will be removed in Umbraco 17.
 */
export type WorkspaceEntityTypeConditionConfig = UmbWorkspaceEntityTypeConditionConfig;

export interface UmbWorkspaceEntityIsNewConditionConfig
	extends UmbConditionConfigBase<typeof UMB_WORKSPACE_ENTITY_IS_NEW_CONDITION_ALIAS> {
	match: boolean;
}

/**
 * @deprecated Use `UmbWorkspaceEntityIsNewConditionConfig` instead. This will be removed in Umbraco 17.
 */
// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface WorkspaceEntityIsNewConditionConfig extends UmbWorkspaceEntityIsNewConditionConfig {}

declare global {
	interface UmbExtensionConditionConfigMap {
		umbWorkspaceAlias: WorkspaceAliasConditionConfig;
		umbWorkspaceEntityIsNewConditionConfig: UmbWorkspaceEntityIsNewConditionConfig;
		umbWorkspaceEntityType: UmbWorkspaceEntityTypeConditionConfig;
	}
}
