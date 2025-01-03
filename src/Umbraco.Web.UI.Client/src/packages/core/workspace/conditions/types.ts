import type {
	UMB_WORKSPACE_CONDITION_ALIAS,
	UMB_WORKSPACE_ENTITY_IS_NEW_CONDITION_ALIAS,
	UMB_WORKSPACE_HAS_COLLECTION_CONDITION_ALIAS,
} from './const.js';
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

export type WorkspaceContentTypeAliasConditionConfig =
	UmbConditionConfigBase<'Umb.Condition.WorkspaceContentTypeAlias'> & {
		/**
		 * Define a content type alias in which workspace this extension should be available
		 * @example
		 * Depends on implementation, but i.e. "article", "image", "blockPage"
		 */
		match?: string;
		/**
		 * Define one or more content type aliases in which workspace this extension should be available
		 * @example
		 * ["article", "image", "blockPage"]
		 */
		oneOf?: Array<string>;
	};

export type WorkspaceEntityTypeConditionConfig = UmbConditionConfigBase<'Umb.Condition.WorkspaceEntityType'> & {
	/**
	 * Define the workspace that this extension should be available in
	 * @example
	 * "Document"
	 */
	match: string;
};

export type WorkspaceHasCollectionConditionConfig = UmbConditionConfigBase<
	typeof UMB_WORKSPACE_HAS_COLLECTION_CONDITION_ALIAS
>;

export interface WorkspaceEntityIsNewConditionConfig
	extends UmbConditionConfigBase<typeof UMB_WORKSPACE_ENTITY_IS_NEW_CONDITION_ALIAS> {
	match: boolean;
}

declare global {
	interface UmbExtensionConditionConfigMap {
		umbWorkspaceAlias: WorkspaceAliasConditionConfig;
		umbWorkspaceContentTypeAlias: WorkspaceContentTypeAliasConditionConfig;
		umbWorkspaceEntityType: WorkspaceEntityTypeConditionConfig;
		umbWorkspaceHasCollection: WorkspaceHasCollectionConditionConfig;
	}
}
