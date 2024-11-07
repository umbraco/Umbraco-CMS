import { UMB_WEBHOOK_ENTITY_TYPE, UMB_WEBHOOK_WORKSPACE_ALIAS } from '../../entity.js';
import { UMB_WORKSPACE_CONDITION_ALIAS, UmbSubmitWorkspaceAction } from '@umbraco-cms/backoffice/workspace';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'workspace',
		kind: 'routable',
		alias: UMB_WEBHOOK_WORKSPACE_ALIAS,
		name: 'Webhook Root Workspace',
		api: () => import('./webhook-workspace.context.js'),
		meta: {
			entityType: UMB_WEBHOOK_ENTITY_TYPE,
		},
	},
	{
		type: 'workspaceView',
		alias: 'Umb.WorkspaceView.Webhook.Details',
		name: 'Webhook Root Workspace Details View',
		js: () => import('./views/webhook-details-workspace-view.element.js'),
		weight: 300,
		meta: {
			label: 'Overview',
			pathname: 'overview',
			icon: 'icon-webhook',
		},
		conditions: [
			{
				alias: UMB_WORKSPACE_CONDITION_ALIAS,
				match: UMB_WEBHOOK_WORKSPACE_ALIAS,
			},
		],
	},
	{
		type: 'workspaceAction',
		kind: 'default',
		alias: 'Umb.WorkspaceAction.Webhook.Save',
		name: 'Save Webhook Workspace Action',
		api: UmbSubmitWorkspaceAction,
		meta: {
			look: 'primary',
			color: 'positive',
			label: '#buttons_save',
		},
		conditions: [
			{
				alias: UMB_WORKSPACE_CONDITION_ALIAS,
				match: UMB_WEBHOOK_WORKSPACE_ALIAS,
			},
		],
	},
];
