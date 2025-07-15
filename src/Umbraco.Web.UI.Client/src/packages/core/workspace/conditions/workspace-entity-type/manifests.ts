import { UMB_WORKSPACE_ENTITY_TYPE_CONDITION_ALIAS } from './constants.js';
import { UmbWorkspaceEntityTypeCondition } from './workspace-entity-type.condition.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'condition',
		name: 'Workspace Entity Type Condition',
		alias: UMB_WORKSPACE_ENTITY_TYPE_CONDITION_ALIAS,
		api: UmbWorkspaceEntityTypeCondition,
	},
];
