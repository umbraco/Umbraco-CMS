import {
	UMB_CONTENT_WORKSPACE_IS_LOADED_CONDITION_ALIAS,
	UMB_WORKSPACE_CONDITION_ALIAS,
	UMB_WORKSPACE_ENTITY_IS_NEW_CONDITION_ALIAS,
} from '@umbraco-cms/backoffice/workspace';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'workspaceContext',
		name: 'Example Name Manipulation Workspace Context',
		alias: 'example.workspaceContext.nameManipulation',
		api: () => import('./workspace-context.js'),
		conditions: [
			{
				alias: UMB_WORKSPACE_CONDITION_ALIAS,
				match: 'Umb.Workspace.Document',
			},
			{
				alias: UMB_WORKSPACE_ENTITY_IS_NEW_CONDITION_ALIAS,
			},
			{
				alias: UMB_CONTENT_WORKSPACE_IS_LOADED_CONDITION_ALIAS,
			},
		],
	},
];
