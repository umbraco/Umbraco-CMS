import { UMB_BLOCK_RTE_TYPE_WORKSPACE_ALIAS } from '../index.js';
import type { ManifestWorkspaceView } from '@umbraco-cms/backoffice/extension-registry';

export const workspaceViews: Array<ManifestWorkspaceView> = [
	{
		type: 'workspaceView',
		alias: 'Umb.WorkspaceView.BlockType.RTE.Settings',
		name: 'Block List Type Workspace Settings View',
		js: () => import('./block-rte-type-workspace-view.element.js'),
		weight: 1000,
		meta: {
			label: 'Settings',
			pathname: 'settings',
			icon: 'icon-settings',
		},
		conditions: [
			{
				alias: 'Umb.Condition.WorkspaceAlias',
				match: UMB_BLOCK_RTE_TYPE_WORKSPACE_ALIAS,
			},
		],
	},
];

export const manifests = [...workspaceViews];
