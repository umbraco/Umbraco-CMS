import type {
	ManifestModal,
	ManifestWorkspace,
	ManifestWorkspaceAction,
	ManifestWorkspaceEditorView,
} from '@umbraco-cms/backoffice/extension-registry';

const workspaceAlias = 'Umb.Workspace.LogViewer';

const workspace: ManifestWorkspace = {
	type: 'workspace',
	alias: workspaceAlias,
	name: 'LogViewer Root Workspace',
	js: () => import('./logviewer-workspace.element.js'),
	meta: {
		entityType: 'logviewer',
	},
};

const workspaceViews: Array<ManifestWorkspaceEditorView> = [
	{
		type: 'workspaceEditorView',
		alias: 'Umb.WorkspaceView.LogViewer.Overview',
		name: 'LogViewer Root Workspace Overview View',
		js: () => import('../views/overview/index.js'),
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
		type: 'workspaceEditorView',
		alias: 'Umb.WorkspaceView.LogViewer.Search',
		name: 'LogViewer Root Workspace Search View',
		js: () => import('../views/search/index.js'),
		weight: 200,
		meta: {
			label: 'Search',
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

const workspaceActions: Array<ManifestWorkspaceAction> = [];

const modals: Array<ManifestModal> = [
	{
		type: 'modal',
		alias: 'Umb.Modal.LogViewer.SaveSearch',
		name: 'Saved Searches Modal',
		js: () => import('../views/search/components/log-viewer-search-input-modal.element.js'),
	},
];

export const manifests = [workspace, ...workspaceViews, ...workspaceActions, ...modals];
