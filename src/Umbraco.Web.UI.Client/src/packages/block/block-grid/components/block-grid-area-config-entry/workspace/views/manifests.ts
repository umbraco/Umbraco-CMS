import { UMB_BLOCK_GRID_AREA_TYPE_WORKSPACE_ALIAS } from '../index.js';
import type { ManifestWorkspaceView } from '@umbraco-cms/backoffice/extension-registry';

export const workspaceViews: Array<ManifestWorkspaceView> = [
	{
		type: 'workspaceView',
		alias: 'Umb.WorkspaceView.BlockGridAreaType.Settings',
		name: 'Block Grid Area Type Workspace Settings View',
		js: () => import('./block-grid-area-type-workspace-view-settings.element.js'),
		weight: 1000,
		meta: {
			label: 'Settings',
			pathname: 'settings',
			icon: 'icon-settings',
		},
		conditions: [
			{
				alias: 'Umb.Condition.WorkspaceAlias',
				match: UMB_BLOCK_GRID_AREA_TYPE_WORKSPACE_ALIAS,
			},
		],
	},
];

export const manifests = [...workspaceViews];
