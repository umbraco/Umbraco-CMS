import { UMB_DATA_TYPE_WORKSPACE_ALIAS } from '../../workspace/constants.js';
import { UMB_WORKSPACE_CONDITION_ALIAS } from '@umbraco-cms/backoffice/workspace';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'workspaceInfoApp',
		name: 'Data Type References Workspace Info App',
		alias: 'Umb.WorkspaceInfoApp.DataType.References',
		element: () => import('./entity-references-workspace-view-info.element.js'),
		weight: 90,
		conditions: [
			{
				alias: UMB_WORKSPACE_CONDITION_ALIAS,
				match: UMB_DATA_TYPE_WORKSPACE_ALIAS,
			},
		],
	},
];
