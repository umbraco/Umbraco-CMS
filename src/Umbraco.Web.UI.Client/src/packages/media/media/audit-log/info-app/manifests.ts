import { UMB_MEDIA_WORKSPACE_ALIAS } from '../../workspace/constants.js';
import { UMB_WORKSPACE_CONDITION_ALIAS } from '@umbraco-cms/backoffice/workspace';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'workspaceInfoApp',
		name: 'Media History Workspace Info App',
		alias: 'Umb.WorkspaceInfoApp.Media.History',
		element: () => import('./media-history-workspace-info-app.element.js'),
		weight: 80,
		conditions: [
			{
				alias: UMB_WORKSPACE_CONDITION_ALIAS,
				match: UMB_MEDIA_WORKSPACE_ALIAS,
			},
		],
	},
];
