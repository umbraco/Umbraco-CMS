import type { UMB_WORKSPACE_HAS_COLLECTION_CONDITION } from './const.js';
import type { UmbConditionConfigBase } from '@umbraco-cms/backoffice/extension-api';

export type WorkspaceAliasConditionConfig = UmbConditionConfigBase<'Umb.Condition.WorkspaceAlias'> & {
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
};

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
	typeof UMB_WORKSPACE_HAS_COLLECTION_CONDITION
>;
