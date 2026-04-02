import { UMB_WORKSPACE_CONTENT_TYPE_ALIAS_CONDITION_ALIAS } from './constants.js';
import { UmbWorkspaceContentTypeAliasCondition } from './workspace-content-type-alias.condition.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'condition',
		name: 'Workspace Content Type Alias Condition',
		alias: UMB_WORKSPACE_CONTENT_TYPE_ALIAS_CONDITION_ALIAS,
		api: UmbWorkspaceContentTypeAliasCondition,
	},
];
