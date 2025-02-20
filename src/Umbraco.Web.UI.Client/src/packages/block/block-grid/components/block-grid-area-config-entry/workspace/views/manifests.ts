import { UMB_BLOCK_GRID_AREA_TYPE_WORKSPACE_ALIAS } from '../constants.js';
import { UMB_WORKSPACE_CONDITION_ALIAS } from '@umbraco-cms/backoffice/workspace';
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
				alias: UMB_WORKSPACE_CONDITION_ALIAS,
				match: UMB_BLOCK_GRID_AREA_TYPE_WORKSPACE_ALIAS,
			},
		],
	},
];

export const manifests: Array<UmbExtensionManifest> = [...workspaceViews];
