import { UMB_WORKSPACE_CONDITION_ALIAS } from '@umbraco-cms/backoffice/workspace';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'workspaceView',
		name: 'Example Badge Workspace View',
		alias: 'example.workspaceView.hint',
		element: () => import('./hint-workspace-view.js'),
		weight: 900,
		meta: {
			label: 'View with badge',
			pathname: 'badge',
			icon: 'icon-lab',
		},
		conditions: [
			{
				alias: UMB_WORKSPACE_CONDITION_ALIAS,
				match: 'Umb.Workspace.Document',
			},
		],
	},
];
