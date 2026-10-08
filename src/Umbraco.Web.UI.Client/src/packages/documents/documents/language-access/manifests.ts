import { UMB_DOCUMENT_WORKSPACE_ALIAS } from '../constants.js';
import { UMB_WORKSPACE_CONDITION_ALIAS } from '@umbraco-cms/backoffice/workspace';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'workspaceContext',
		kind: 'contentLanguageAccess',
		name: 'Document Language Access Workspace Context',
		alias: 'Umb.WorkspaceContext.Document.LanguageAccess',
		conditions: [
			{
				alias: UMB_WORKSPACE_CONDITION_ALIAS,
				match: UMB_DOCUMENT_WORKSPACE_ALIAS,
			},
		],
	},
];
