import type { ManifestWorkspaceView } from '@umbraco-cms/backoffice/extension-registry';

const workspaceViews: Array<ManifestWorkspaceView> = [
	{
		type: 'workspaceView',
		alias: 'Umb.WorkspaceView.Webhooks.Overview',
		name: 'Webhooks Root Workspace Overview View',
		js: () => import('./views/index.js'),
		weight: 300,
		meta: {
			label: 'Overview',
			pathname: 'overview',
			icon: 'icon-webhook',
		},
		conditions: [
			{
				alias: 'Umb.Condition.WorkspaceAlias',
				match: 'Umb.Workspace.Webhooks',
			},
		],
	},
	/*{
		type: 'workspaceView',
		alias: 'Umb.WorkspaceView.Webhooks.Search',
		name: 'Webhooks Root Workspace Logs View',
		js: () => import('../views/logs/index.js'),
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
	},*/
];

export const manifests = [...workspaceViews];
