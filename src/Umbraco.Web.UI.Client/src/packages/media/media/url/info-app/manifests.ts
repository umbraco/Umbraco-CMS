import { UMB_MEDIA_WORKSPACE_ALIAS } from '../../workspace/constants.js';
import { UMB_WORKSPACE_CONDITION_ALIAS } from '@umbraco-cms/backoffice/workspace';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'workspaceInfoApp',
		name: 'Media Links Workspace Info App',
		alias: 'Umb.WorkspaceInfoApp.Media.Links',
		element: () => import('./media-links-workspace-info-app.element.js'),
		weight: 100,
		conditions: [
			{
				alias: UMB_WORKSPACE_CONDITION_ALIAS,
				match: UMB_MEDIA_WORKSPACE_ALIAS,
			},
		],
	},
];
