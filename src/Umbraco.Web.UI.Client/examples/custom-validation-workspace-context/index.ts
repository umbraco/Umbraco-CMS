import { UMB_WORKSPACE_CONTENT_TYPE_ALIAS_CONDITION_ALIAS } from '@umbraco-cms/backoffice/content-type';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'workspaceContext',
		name: 'Example Custom Validation Workspace Context',
		alias: 'example.workspaceCounter.customValidation',
		api: () => import('./custom-validation-workspace-context.js'),
		conditions: [
			{
				alias: UMB_WORKSPACE_CONTENT_TYPE_ALIAS_CONDITION_ALIAS,
				match: 'allRtesDocumentType',
			},
		],
	},
];
