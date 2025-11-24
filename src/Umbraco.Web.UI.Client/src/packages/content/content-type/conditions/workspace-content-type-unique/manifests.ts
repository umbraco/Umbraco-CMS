import { UMB_WORKSPACE_CONTENT_TYPE_UNIQUE_CONDITION } from './constants.js';
import { UmbWorkspaceContentTypeUniqueCondition } from './workspace-content-type-unique.condition.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'condition',
		name: 'Workspace Content Type Unique Condition',
		alias: UMB_WORKSPACE_CONTENT_TYPE_UNIQUE_CONDITION,
		api: UmbWorkspaceContentTypeUniqueCondition,
	},
];
