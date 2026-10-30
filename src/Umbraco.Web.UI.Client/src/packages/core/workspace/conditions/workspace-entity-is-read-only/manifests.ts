import { UMB_WORKSPACE_ENTITY_IS_READ_ONLY_CONDITION_ALIAS } from './constants.js';
import { UmbWorkspaceEntityIsReadOnlyCondition } from './workspace-entity-is-read-only.condition.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'condition',
		name: 'Workspace Entity Is Read-only Condition',
		alias: UMB_WORKSPACE_ENTITY_IS_READ_ONLY_CONDITION_ALIAS,
		api: UmbWorkspaceEntityIsReadOnlyCondition,
	},
];
