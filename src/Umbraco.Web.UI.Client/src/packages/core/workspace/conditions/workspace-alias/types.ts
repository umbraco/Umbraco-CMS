import type { UMB_WORKSPACE_CONDITION_ALIAS } from './constants.js';
import type { UmbConditionConfigBase } from '@umbraco-cms/backoffice/extension-api';

// TODO: Rename this to `UmbWorkspaceAliasConditionConfig` in a future version.
// eslint-disable-next-line @typescript-eslint/naming-convention
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

declare global {
	interface UmbExtensionConditionConfigMap {
		umbWorkspaceAlias: WorkspaceAliasConditionConfig;
	}
}
