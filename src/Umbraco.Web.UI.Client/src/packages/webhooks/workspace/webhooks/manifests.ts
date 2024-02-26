import type {
	ManifestWorkspace,
	ManifestWorkspaceAction,
	ManifestWorkspaceView,
} from '@umbraco-cms/backoffice/extension-registry';

const workspaceAlias = 'Umb.Workspace.Webhooks';

const workspace: ManifestWorkspace = {
	type: 'workspace',
	alias: workspaceAlias,
	name: 'Webhooks Root Workspace',
	js: () => import('./webhooks-workspace.element.js'),
	meta: {
		entityType: 'webhooks',
	},
};

const workspaceViews: Array<ManifestWorkspaceView> = [
	{
		type: 'workspaceView',
		alias: 'Umb.WorkspaceView.Webhook.Overview',
		name: 'Webhooks Root Workspace Overview View',
		js: () => import('../views/webhook-details-workspace-view.js'),
		weight: 300,
		meta: {
			label: 'Webhooks',
			pathname: 'webhooks',
			icon: 'anchor',
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
		alias: 'Umb.WorkspaceView.Webhook.Logs',
		name: 'Webhooks Root Workspace Logs View',
		js: () => import('../views/logs/index.js'),
		weight: 200,
		meta: {
			label: 'Logs',
			pathname: 'logs',
			icon: 'box-alt',
		},
		conditions: [
			{
				alias: 'Umb.Condition.WorkspaceAlias',
				match: workspace.alias,
			},
		],
	},
];

const workspaceActions: Array<ManifestWorkspaceAction> = [];

export const manifests = [workspace, ...workspaceViews, ...workspaceActions];
