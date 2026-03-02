import { UMB_DOCUMENT_BLUEPRINT_WORKSPACE_ALIAS } from '../../workspace/constants.js';
import { UMB_WORKSPACE_CONDITION_ALIAS } from '@umbraco-cms/backoffice/workspace';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'workspaceInfoApp',
		name: 'Document Blueprint History Workspace Info App',
		alias: 'Umb.WorkspaceInfoApp.DocumentBlueprint.History',
		element: () => import('./document-blueprint-history-workspace-info-app.element.js'),
		weight: 80,
		conditions: [
			{
				alias: UMB_WORKSPACE_CONDITION_ALIAS,
				match: UMB_DOCUMENT_BLUEPRINT_WORKSPACE_ALIAS,
			},
		],
	},
];
