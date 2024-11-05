import { UMB_WORKSPACE_CONDITION_ALIAS } from '@umbraco-cms/backoffice/workspace';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'workspaceContext',
		name: 'Example Counter Workspace Context',
		alias: 'example.workspaceCounter.counter',
		api: () => import('./counter-workspace-context.js'),
		conditions: [
			{
				alias: UMB_WORKSPACE_CONDITION_ALIAS,
				match: 'Umb.Workspace.Document',
			},
		],
	},
	{
		type: 'workspaceAction',
		kind: 'default',
		name: 'Example Count Incrementor Workspace Action',
		alias: 'example.workspaceAction.incrementor',
		weight: 1000,
		api: () => import('./incrementor-workspace-action.js'),
		meta: {
			label: 'Increment',
			look: 'primary',
			color: 'danger',
		},
		conditions: [
			{
				alias: UMB_WORKSPACE_CONDITION_ALIAS,
				match: 'Umb.Workspace.Document',
			},
		],
	},
	{
		type: 'workspaceView',
		name: 'Example Counter Workspace View',
		alias: 'example.workspaceView.counter',
		element: () => import('./counter-workspace-view.js'),
		weight: 900,
		meta: {
			label: 'Counter',
			pathname: 'counter',
			icon: 'icon-lab',
		},
		conditions: [
			{
				alias: UMB_WORKSPACE_CONDITION_ALIAS,
				match: 'Umb.Workspace.Document',
			},
		],
	},
];
