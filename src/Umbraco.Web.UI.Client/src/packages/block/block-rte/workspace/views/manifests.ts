import { UMB_BLOCK_RTE_TYPE_WORKSPACE_ALIAS } from '../index.js';
import { UMB_WORKSPACE_CONDITION_ALIAS } from '@umbraco-cms/backoffice/workspace';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'workspaceView',
		alias: 'Umb.WorkspaceView.BlockType.RTE.Settings',
		name: 'Block List Type Workspace Settings View',
		element: () => import('./block-rte-type-workspace-view.element.js'),
		weight: 1000,
		meta: {
			label: '#general_settings',
			pathname: 'settings',
			icon: 'icon-settings',
		},
		conditions: [
			{
				alias: UMB_WORKSPACE_CONDITION_ALIAS,
				match: UMB_BLOCK_RTE_TYPE_WORKSPACE_ALIAS,
			},
		],
	},
];
