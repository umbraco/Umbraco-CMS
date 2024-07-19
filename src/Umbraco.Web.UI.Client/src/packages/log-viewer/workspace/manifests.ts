import type {
	ManifestModal,
	ManifestTypes,
	ManifestWorkspace,
	ManifestWorkspaceActions,
	ManifestWorkspaceView,
} from '@umbraco-cms/backoffice/extension-registry';

const workspaceAlias = 'Umb.Workspace.LogViewer';

const workspace: ManifestWorkspace = {
	type: 'workspace',
	alias: workspaceAlias,
	name: 'LogViewer Root Workspace',
	element: () => import('./logviewer-workspace.element.js'),
	api: () => import('./logviewer-workspace.context.js'),
	meta: {
		entityType: 'logviewer',
	},
};

const workspaceViews: Array<ManifestWorkspaceView> = [
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
				alias: 'Umb.Condition.WorkspaceAlias',
				match: workspace.alias,
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
				alias: 'Umb.Condition.WorkspaceAlias',
				match: workspace.alias,
			},
		],
	},
];

const workspaceActions: Array<ManifestWorkspaceActions> = [];

const modals: Array<ManifestModal> = [
	{
		type: 'modal',
		alias: 'Umb.Modal.LogViewer.SaveSearch',
		name: 'Saved Searches Modal',
		element: () => import('./views/search/components/log-viewer-search-input-modal.element.js'),
	},
];

export const manifests: Array<ManifestTypes> = [workspace, ...workspaceViews, ...workspaceActions, ...modals];
