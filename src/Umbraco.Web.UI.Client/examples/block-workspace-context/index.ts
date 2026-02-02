import { UMB_BLOCK_WORKSPACE_ALIAS } from '@umbraco-cms/backoffice/block';
import { UMB_WORKSPACE_CONDITION_ALIAS } from '@umbraco-cms/backoffice/workspace';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'workspaceView',
		name: 'Example Block Workspace View',
		alias: 'example.workspaceView.block',
		element: () => import('./block-workspace-view.js'),
		weight: 900,
		meta: {
			label: 'Counter',
			pathname: 'counter',
			icon: 'icon-lab',
		},
		conditions: [
			{
				alias: UMB_WORKSPACE_CONDITION_ALIAS,
				match: UMB_BLOCK_WORKSPACE_ALIAS,
			},
		],
	},
];
