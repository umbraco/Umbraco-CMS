import { UMB_BLOCK_WORKSPACE_ALIAS } from './index.js';
import { UmbSaveWorkspaceAction } from '@umbraco-cms/backoffice/workspace';
import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<ManifestTypes> = [
	{
		type: 'workspaceAction',
		alias: 'Umb.WorkspaceAction.Block.Save',
		name: 'Save Block Type Workspace Action',
		api: UmbSaveWorkspaceAction,
		meta: {
			label: 'Submit',
			look: 'primary',
			color: 'positive',
		},
		conditions: [
			{
				alias: 'Umb.Condition.WorkspaceAlias',
				oneOf: [UMB_BLOCK_WORKSPACE_ALIAS],
			},
		],
	},
	{
		type: 'workspace',
		name: 'Block List Type Workspace',
		alias: UMB_BLOCK_WORKSPACE_ALIAS,
		element: () => import('./block-workspace.element.js'),
		api: () => import('./block-workspace.context.js'),
		weight: 900,
		meta: {
			entityType: 'block',
		},
	},
	{
		type: 'workspaceView',
		alias: 'Umb.WorkspaceView.Block.Content',
		name: 'Block Workspace Content View',
		js: () => import('./views/edit/block-workspace-view-edit.element.js'),
		weight: 1000,
		meta: {
			label: 'Content',
			pathname: 'content',
			icon: 'icon-document',
			blockElementManagerName: 'content',
		},
		conditions: [
			{
				alias: 'Umb.Condition.WorkspaceAlias',
				match: UMB_BLOCK_WORKSPACE_ALIAS,
			},
		],
	} as any,
	{
		type: 'workspaceView',
		alias: 'Umb.WorkspaceView.Block.Settings',
		name: 'Block Workspace Settings View',
		js: () => import('./views/edit/block-workspace-view-edit.element.js'),
		weight: 1000,
		meta: {
			label: 'Settings',
			pathname: 'settings',
			icon: 'icon-settings',
			blockElementManagerName: 'settings',
		},
		conditions: [
			{
				alias: 'Umb.Condition.WorkspaceAlias',
				match: UMB_BLOCK_WORKSPACE_ALIAS,
			},
			{
				alias: 'Umb.Condition.BlockHasSettings',
			},
		],
	} as any,
];
