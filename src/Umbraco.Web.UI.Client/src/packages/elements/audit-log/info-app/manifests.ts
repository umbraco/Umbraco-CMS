import { UMB_ELEMENT_WORKSPACE_ALIAS } from '../../workspace/constants.js';
import { UMB_WORKSPACE_CONDITION_ALIAS } from '@umbraco-cms/backoffice/workspace';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'workspaceInfoApp',
		name: 'Element History Workspace Info App',
		alias: 'Umb.WorkspaceInfoApp.Element.History',
		element: () => import('./element-history-workspace-info-app.element.js'),
		weight: 80,
		conditions: [
			{
				alias: UMB_WORKSPACE_CONDITION_ALIAS,
				match: UMB_ELEMENT_WORKSPACE_ALIAS,
			},
		],
	},
];
