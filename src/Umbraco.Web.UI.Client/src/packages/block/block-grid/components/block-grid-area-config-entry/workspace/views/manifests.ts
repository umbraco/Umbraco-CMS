import { UMB_BLOCK_GRID_AREA_TYPE_WORKSPACE_ALIAS } from '../index.js';
import type { ManifestWorkspaceView } from '@umbraco-cms/backoffice/workspace';

export const workspaceViews: Array<ManifestWorkspaceView> = [
	{
		type: 'workspaceView',
		alias: 'Umb.WorkspaceView.BlockGridAreaType.Settings',
		name: 'Block Grid Area Type Workspace Settings View',
		element: () => import('./settings.element.js'),
		weight: 1000,
		meta: {
			label: '#general_settings',
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

export const manifests: Array<UmbExtensionManifest> = [...workspaceViews];
