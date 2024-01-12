import { UMB_BLOCK_GRID_TYPE_WORKSPACE_ALIAS } from '../index.js';
import type { ManifestWorkspaceView } from '@umbraco-cms/backoffice/extension-registry';

export const workspaceViews: Array<ManifestWorkspaceView> = [
	{
		type: 'workspaceView',
		alias: 'Umb.WorkspaceView.BlockType.Grid.Settings',
		name: 'Block Grid Type Workspace Settings View',
		js: () => import('./block-grid-type-workspace-view-settings.element.js'),
		weight: 1000,
		meta: {
			label: 'Settings',
			pathname: 'settings',
			icon: 'icon-settings',
		},
		conditions: [
			{
				alias: 'Umb.Condition.WorkspaceAlias',
				match: UMB_BLOCK_GRID_TYPE_WORKSPACE_ALIAS,
			},
		],
	},
	{
		type: 'workspaceView',
		alias: 'Umb.WorkspaceView.BlockType.Grid.Areas',
		name: 'Block Grid Type Workspace Areas View',
		js: () => import('./block-grid-type-workspace-view-areas.element.js'),
		weight: 1000,
		meta: {
			label: 'Areas',
			pathname: 'areas',
			icon: 'icon-grid',
		},
		conditions: [
			{
				alias: 'Umb.Condition.WorkspaceAlias',
				match: UMB_BLOCK_GRID_TYPE_WORKSPACE_ALIAS,
			},
		],
	},
	{
		type: 'workspaceView',
		alias: 'Umb.WorkspaceView.BlockType.Grid.Advance',
		name: 'Block Grid Type Workspace Advance View',
		js: () => import('./block-grid-type-workspace-view-advanced.element.js'),
		weight: 1000,
		meta: {
			label: 'Advanced',
			pathname: 'advanced',
			icon: 'icon-wrench',
		},
		conditions: [
			{
				alias: 'Umb.Condition.WorkspaceAlias',
				match: UMB_BLOCK_GRID_TYPE_WORKSPACE_ALIAS,
			},
		],
	},
];

export const manifests = [...workspaceViews];
