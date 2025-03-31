import { UMB_MEMBER_WORKSPACE_ALIAS } from '../../workspace/constants.js';
import { UMB_WORKSPACE_CONDITION_ALIAS } from '@umbraco-cms/backoffice/workspace';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'workspaceInfoApp',
		name: 'Member References Workspace Info App',
		alias: 'Umb.WorkspaceInfoApp.Member.References',
		element: () => import('./member-references-workspace-info-app.element.js'),
		weight: 90,
		conditions: [
			{
				alias: UMB_WORKSPACE_CONDITION_ALIAS,
				match: UMB_MEMBER_WORKSPACE_ALIAS,
			},
		],
	},
];
