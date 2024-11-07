import { UMB_BLOCK_GRID_TYPE_WORKSPACE_ALIAS } from '../index.js';
import { UMB_WORKSPACE_CONDITION_ALIAS } from '@umbraco-cms/backoffice/workspace';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'workspaceView',
		alias: 'Umb.WorkspaceView.BlockType.Grid.Settings',
		name: 'Block Grid Type Workspace Settings View',
		element: () => import('./block-grid-type-workspace-view-settings.element.js'),
		weight: 1000,
		meta: {
			label: '#general_settings',
			pathname: 'settings',
			icon: 'icon-settings',
		},
		conditions: [
			{
				alias: UMB_WORKSPACE_CONDITION_ALIAS,
				match: UMB_BLOCK_GRID_TYPE_WORKSPACE_ALIAS,
			},
		],
	},
	{
		type: 'workspaceView',
		alias: 'Umb.WorkspaceView.BlockType.Grid.Areas',
		name: 'Block Grid Type Workspace Areas View',
		element: () => import('./block-grid-type-workspace-view-areas.element.js'),
		weight: 1000,
		meta: {
			label: '#blockEditor_tabAreas',
			pathname: 'areas',
			icon: 'icon-grid',
		},
		conditions: [
			{
				alias: UMB_WORKSPACE_CONDITION_ALIAS,
				match: UMB_BLOCK_GRID_TYPE_WORKSPACE_ALIAS,
			},
		],
	},
	{
		type: 'workspaceView',
		alias: 'Umb.WorkspaceView.BlockType.Grid.Advance',
		name: 'Block Grid Type Workspace Advance View',
		element: () => import('./block-grid-type-workspace-view-advanced.element.js'),
		weight: 1000,
		meta: {
			label: '#blockEditor_tabAdvanced',
			pathname: 'advanced',
			icon: 'icon-wrench',
		},
		conditions: [
			{
				alias: UMB_WORKSPACE_CONDITION_ALIAS,
				match: UMB_BLOCK_GRID_TYPE_WORKSPACE_ALIAS,
			},
		],
	},
];
