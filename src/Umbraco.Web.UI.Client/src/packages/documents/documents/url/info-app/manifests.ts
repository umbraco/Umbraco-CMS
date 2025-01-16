import { UMB_DOCUMENT_WORKSPACE_ALIAS } from '../../workspace/constants.js';
import { UMB_WORKSPACE_CONDITION_ALIAS } from '@umbraco-cms/backoffice/workspace';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'workspaceInfoApp',
		name: 'Document Links Workspace Info App',
		alias: 'Umb.WorkspaceInfoApp.Document.Links',
		element: () => import('./document-links-workspace-info-app.element.js'),
		weight: 100,
		meta: {
			label: 'Links',
		},
		conditions: [
			{
				alias: UMB_WORKSPACE_CONDITION_ALIAS,
				match: UMB_DOCUMENT_WORKSPACE_ALIAS,
			},
		],
	},
];
