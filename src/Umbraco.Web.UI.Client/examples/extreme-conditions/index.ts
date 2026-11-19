import { UMB_ENTITY_UNIQUE_CONDITION_ALIAS } from '@umbraco-cms/backoffice/entity';
import { UMB_WORKSPACE_ENTITY_IS_NEW_CONDITION_ALIAS } from '@umbraco-cms/backoffice/workspace';

// Uniques of documents in the mock data set (`npm run example:mock`). Replace with the uniques of your own documents.
const SLOW_AND_UNIQUE_DOCUMENT = 'the-simplest-document-id';

// Longer than the time a workspace waits for its Workspace Contexts, so the wait is visible.
const SLOW_CONDITION_OFFSET_MS = '10000';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'workspaceContext',
		name: 'Example Extreme Conditions: Slow And Unique Workspace Context',
		alias: 'example.workspaceContext.extremeConditions.slowAndUnique',
		api: () => import('./extreme-conditions-workspace-context.js'),
		conditions: [
			{
				alias: 'Umb.Condition.Delay',
				offset: SLOW_CONDITION_OFFSET_MS,
			},
			{
				alias: UMB_ENTITY_UNIQUE_CONDITION_ALIAS,
				match: SLOW_AND_UNIQUE_DOCUMENT,
			},
		],
	},
	{
		type: 'workspaceContext',
		name: 'Example Extreme Conditions: Slow And Negative Workspace Context',
		alias: 'example.workspaceContext.extremeConditions.slowAndNegative',
		api: () => import('./extreme-conditions-workspace-context.js'),
		conditions: [
			{
				alias: 'Umb.Condition.Delay',
				offset: SLOW_CONDITION_OFFSET_MS,
			},
			{
				// Negative for every document that is already saved.
				alias: UMB_WORKSPACE_ENTITY_IS_NEW_CONDITION_ALIAS,
			},
		],
	},
];
