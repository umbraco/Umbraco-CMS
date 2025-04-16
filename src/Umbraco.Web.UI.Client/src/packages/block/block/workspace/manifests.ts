import { UMB_BLOCK_WORKSPACE_ALIAS } from './index.js';
import { UMB_WORKSPACE_CONDITION_ALIAS, UmbSubmitWorkspaceAction } from '@umbraco-cms/backoffice/workspace';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'workspaceAction',
		kind: 'default',
		alias: 'Umb.WorkspaceAction.Block.SubmitCreate',
		name: 'Submit Create Block Workspace Action',
		api: UmbSubmitWorkspaceAction,
		meta: {
			label: '#general_create',
			look: 'primary',
			color: 'positive',
		},
		conditions: [
			{
				alias: UMB_WORKSPACE_CONDITION_ALIAS,
				match: UMB_BLOCK_WORKSPACE_ALIAS,
			},
			{
				alias: 'Umb.Condition.BlockWorkspaceIsExposed',
				match: false,
			},
		],
	},
	{
		type: 'workspaceAction',
		kind: 'default',
		alias: 'Umb.WorkspaceAction.Block.SubmitUpdate',
		name: 'Submit Update Block Workspace Action',
		api: UmbSubmitWorkspaceAction,
		meta: {
			label: '#general_update',
			look: 'primary',
			color: 'positive',
		},
		conditions: [
			{
				alias: UMB_WORKSPACE_CONDITION_ALIAS,
				match: UMB_BLOCK_WORKSPACE_ALIAS,
			},
			{
				alias: 'Umb.Condition.BlockWorkspaceIsExposed',
			},
		],
	},
	{
		type: 'workspace',
		kind: 'routable',
		name: 'Block Workspace',
		alias: UMB_BLOCK_WORKSPACE_ALIAS,
		api: () => import('./block-workspace.context.js'),
		meta: {
			entityType: 'block',
		},
	},
	{
		type: 'workspaceView',
		alias: 'Umb.WorkspaceView.Block.Content',
		name: 'Block Workspace Content View',
		element: () => import('./views/edit/block-workspace-view-edit.element.js'),
		weight: 1000,
		meta: {
			label: '#general_content',
			pathname: 'content',
			icon: 'icon-document',
			blockElementManagerName: 'content',
		},
		conditions: [
			{
				alias: UMB_WORKSPACE_CONDITION_ALIAS,
				match: UMB_BLOCK_WORKSPACE_ALIAS,
			},
		],
		TODO_conditions: [
			{
				alias: 'Umb.Condition.BlockEntryShowContentEdit',
			},
		],
	} as any,
	// TODO: Fix manifest types so it support additional properties.
	{
		type: 'workspaceView',
		alias: 'Umb.WorkspaceView.Block.Settings',
		name: 'Block Workspace Settings View',
		element: () => import('./views/edit/block-workspace-view-edit.element.js'),
		weight: 900,
		meta: {
			label: '#general_settings',
			pathname: 'settings',
			icon: 'icon-settings',
			blockElementManagerName: 'settings',
		},
		conditions: [
			{
				alias: UMB_WORKSPACE_CONDITION_ALIAS,
				match: UMB_BLOCK_WORKSPACE_ALIAS,
			},
			{
				alias: 'Umb.Condition.BlockWorkspaceHasSettings',
			},
		],
	} as any,
];
