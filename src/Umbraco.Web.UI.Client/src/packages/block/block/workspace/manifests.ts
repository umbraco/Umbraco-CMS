import { UMB_BLOCK_WORKSPACE_ALIAS } from './index.js';
import { UmbSubmitWorkspaceAction } from '@umbraco-cms/backoffice/workspace';
import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<ManifestTypes> = [
	{
		type: 'workspaceAction',
		kind: 'default',
		alias: 'Umb.WorkspaceAction.Block.Save',
		name: 'Save Block Type Workspace Action',
		api: UmbSubmitWorkspaceAction,
		meta: {
			label: '#general_submit',
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
		kind: 'routable',
		name: 'Block List Type Workspace',
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
		js: () => import('./views/edit/block-workspace-view-edit.element.js'),
		weight: 1000,
		meta: {
			label: '#general_content',
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
		js: () => import('./views/edit/block-workspace-view-edit.element.js'),
		weight: 1000,
		meta: {
			label: '#general_settings',
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
				alias: 'Umb.Condition.BlockWorkspaceHasSettings',
			},
		],
	} as any,
];
