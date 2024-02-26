import type {
	ManifestWorkspace,
	ManifestWorkspaceAction,
	ManifestWorkspaceEditorView,
} from '@umbraco-cms/backoffice/extension-registry';

const workspaceAlias = 'Umb.Workspace.Webhooks';

const workspace: ManifestWorkspace = {
	type: 'workspace',
	alias: workspaceAlias,
	name: 'Webhooks Root Workspace',
	loader: () => import('./webhooks-workspace.element.js'),
	meta: {
		entityType: 'webhooks',
	},
};

const workspaceViews: Array<ManifestWorkspaceEditorView> = [
	{
		type: 'workspaceEditorView',
		alias: 'Umb.WorkspaceView.Webhooks.Overview',
		name: 'Webhooks Root Workspace Overview View',
		loader: () => import('../views/overview/index.js'),
		weight: 300,
		meta: {
			label: 'Webhooks',
			pathname: 'webhooks',
			icon: 'umb:anchor',
		},
		conditions: [
			{
				alias: 'Umb.Condition.WorkspaceAlias',
				match: workspace.alias,
			},
		],
	},
	{
		type: 'workspaceEditorView',
		alias: 'Umb.WorkspaceView.Webhooks.Logs',
		name: 'Webhooks Root Workspace Logs View',
		loader: () => import('../views/logs/index.js'),
		weight: 200,
		meta: {
			label: 'Logs',
			pathname: 'logs',
			icon: 'umb:box-alt',
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
