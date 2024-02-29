import { UMB_WEBHOOK_ENTITY_TYPE, UMB_WEBHOOK_WORKSPACE } from '../../entity.js';
import type { ManifestWorkspace, ManifestWorkspaceView } from '@umbraco-cms/backoffice/extension-registry';

const workspace: ManifestWorkspace = {
	type: 'workspace',
	alias: UMB_WEBHOOK_WORKSPACE,
	name: 'Webhook Root Workspace',
	js: () => import('./webhook-workspace.element.js'),
	meta: {
		entityType: UMB_WEBHOOK_ENTITY_TYPE,
	},
};

const workspaceViews: Array<ManifestWorkspaceView> = [
	{
		type: 'workspaceView',
		alias: 'Umb.WorkspaceView.Webhook.Overview',
		name: 'Webhook Root Workspace Overview View',
		js: () => import('../views/overview/webhook-overview-view.element.js'),
		weight: 300,
		meta: {
			label: 'Overview',
			pathname: 'overview',
			icon: 'icon-webhook',
		},
		conditions: [
			{
				alias: 'Umb.Condition.WorkspaceAlias',
				match: workspace.alias,
			},
		],
	},
	{
		type: 'workspaceView',
		alias: 'Umb.WorkspaceView.Webhook.Search',
		name: 'Webhook Root Workspace Logs View',
		js: () => import('../views/overview/webhook-overview-view.element.js'),
		weight: 200,
		meta: {
			label: 'Logs',
			pathname: 'logs',
			icon: 'icon-box-alt',
		},
		conditions: [
			{
				alias: 'Umb.Condition.WorkspaceAlias',
				match: workspace.alias,
			},
		],
	},
];

export const manifests = [workspace, ...workspaceViews];
