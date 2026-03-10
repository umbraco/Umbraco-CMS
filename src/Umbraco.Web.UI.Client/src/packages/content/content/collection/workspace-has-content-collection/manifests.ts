import { UMB_WORKSPACE_HAS_CONTENT_COLLECTION_CONDITION_ALIAS } from './constants.js';
import { UmbWorkspaceHasContentCollectionCondition } from './workspace-has-content-collection.condition.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'condition',
		name: 'Workspace Has Collection Condition',
		alias: UMB_WORKSPACE_HAS_CONTENT_COLLECTION_CONDITION_ALIAS,
		api: UmbWorkspaceHasContentCollectionCondition,
	},
];
