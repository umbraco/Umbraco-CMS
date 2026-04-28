import { UMB_DOCUMENT_WORKSPACE_ALIAS } from '../../workspace/constants.js';
import {
	UMB_WORKSPACE_CONDITION_ALIAS,
	UMB_WORKSPACE_ENTITY_IS_NEW_CONDITION_ALIAS,
} from '@umbraco-cms/backoffice/workspace';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'workspaceInfoApp',
		name: 'Document Redirect Management Workspace Info App',
		alias: 'Umb.WorkspaceInfoApp.Document.RedirectManagement',
		element: () => import('./document-redirect-management-workspace-info-app.element.js'),
		weight: 80,
		conditions: [
			{
				alias: UMB_WORKSPACE_CONDITION_ALIAS,
				match: UMB_DOCUMENT_WORKSPACE_ALIAS,
			},
			{
				alias: UMB_WORKSPACE_ENTITY_IS_NEW_CONDITION_ALIAS,
				match: false,
			},
		],
	},
];
