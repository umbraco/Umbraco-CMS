import { UMB_WORKSPACE_CONDITION_ALIAS } from './constants.js';
import { UmbWorkspaceAliasCondition } from './workspace-alias.condition.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'condition',
		name: 'Workspace Alias Condition',
		alias: UMB_WORKSPACE_CONDITION_ALIAS,
		api: UmbWorkspaceAliasCondition,
	},
];
