import { UMB_MEDIA_WORKSPACE_ALIAS } from '../../workspace/constants.js';
import { UMB_WORKSPACE_CONDITION_ALIAS } from '@umbraco-cms/backoffice/workspace';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'workspaceInfoApp',
		name: 'Media References Workspace Info App',
		alias: 'Umb.WorkspaceInfoApp.Media.References',
		element: () => import('./media-references-workspace-info-app.element.js'),
		weight: 90,
		conditions: [
			{
				alias: UMB_WORKSPACE_CONDITION_ALIAS,
				match: UMB_MEDIA_WORKSPACE_ALIAS,
			},
		],
	},
];
