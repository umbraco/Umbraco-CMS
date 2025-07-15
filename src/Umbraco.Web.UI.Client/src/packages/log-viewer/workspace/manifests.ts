import { UMB_WORKSPACE_CONDITION_ALIAS } from '@umbraco-cms/backoffice/workspace';

const UMB_LOG_VIEWER_WORKSPACE_ALIAS = 'Umb.Workspace.LogViewer';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'workspace',
		alias: UMB_LOG_VIEWER_WORKSPACE_ALIAS,
		name: 'LogViewer Root Workspace',
		element: () => import('./logviewer-workspace.element.js'),
		api: () => import('./logviewer-workspace.context.js'),
		meta: {
			entityType: 'logviewer',
		},
	},
	{
		type: 'workspaceView',
		alias: 'Umb.WorkspaceView.LogViewer.Overview',
		name: 'LogViewer Root Workspace Overview View',
		element: () => import('./views/overview/index.js'),
		weight: 300,
		meta: {
			label: 'Overview',
			pathname: 'overview',
			icon: 'icon-box-alt',
		},
		conditions: [
			{
				alias: UMB_WORKSPACE_CONDITION_ALIAS,
				match: UMB_LOG_VIEWER_WORKSPACE_ALIAS,
			},
		],
	},
	{
		type: 'workspaceView',
		alias: 'Umb.WorkspaceView.LogViewer.Search',
		name: 'LogViewer Root Workspace Search View',
		element: () => import('./views/search/index.js'),
		weight: 200,
		meta: {
			label: '#general_search',
			pathname: 'search',
			icon: 'icon-search',
		},
		conditions: [
			{
				alias: UMB_WORKSPACE_CONDITION_ALIAS,
				match: UMB_LOG_VIEWER_WORKSPACE_ALIAS,
			},
		],
	},
	{
		type: 'modal',
		alias: 'Umb.Modal.LogViewer.SaveSearch',
		name: 'Saved Searches Modal',
		element: () => import('./views/search/components/log-viewer-search-input-modal.element.js'),
	},
];
