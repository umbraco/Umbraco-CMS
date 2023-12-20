import { UMB_BLOCK_TYPE_WORKSPACE_ALIAS } from '../block-type-workspace-alias.const.js';
import { UmbSaveWorkspaceAction } from '@umbraco-cms/backoffice/workspace';
import type { ManifestWorkspaceAction, ManifestWorkspaceView } from '@umbraco-cms/backoffice/extension-registry';

const workspaceActions: Array<ManifestWorkspaceAction> = [
	{
		type: 'workspaceAction',
		alias: 'Umb.WorkspaceAction.BlockType.Save',
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
				match: UMB_BLOCK_TYPE_WORKSPACE_ALIAS,
			},
		],
	},
];

export const workspaceViews: Array<ManifestWorkspaceView> = [
	{
		type: 'workspaceView',
		alias: 'Umb.WorkspaceView.BlockType.List.Settings',
		name: 'Document Type Workspace Design View',
		js: () => import('./block-type-list-workspace-view.element.js'),
		weight: 1000,
		meta: {
			label: 'Settings',
			pathname: 'settings',
			icon: 'icon-settings',
		},
		conditions: [
			{
				alias: 'Umb.Condition.WorkspaceAlias',
				match: UMB_BLOCK_TYPE_WORKSPACE_ALIAS,
			},
		],
	},
];

export const manifests = [...workspaceViews, ...workspaceActions];
