import { UMB_WORKSPACE_ENTITY_IS_NEW_CONDITION_ALIAS } from './constants.js';
import { UmbWorkspaceEntityIsNewCondition } from './workspace-entity-is-new.condition.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'condition',
		name: 'Workspace Entity Is New Condition',
		alias: UMB_WORKSPACE_ENTITY_IS_NEW_CONDITION_ALIAS,
		api: UmbWorkspaceEntityIsNewCondition,
	},
];
