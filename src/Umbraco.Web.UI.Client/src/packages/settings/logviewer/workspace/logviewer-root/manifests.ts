import type {
	ManifestModal,
	ManifestWorkspace,
	ManifestWorkspaceAction,
	ManifestWorkspaceEditorView,
} from '@umbraco-cms/backoffice/extension-registry';

const workspaceAlias = 'Umb.Workspace.LogviewerRoot';

const workspace: ManifestWorkspace = {
	type: 'workspace',
	alias: workspaceAlias,
	name: 'LogViewer Root Workspace',
	loader: () => import('./logviewer-root-workspace.element.js'),
	meta: {
		entityType: 'logviewer',
	},
};

const workspaceViews: Array<ManifestWorkspaceEditorView> = [
	{
		type: 'workspaceEditorView',
		alias: 'Umb.WorkspaceView.Logviewer.Overview',
		name: 'LogViewer Root Workspace Overview View',
		loader: () => import('../views/overview/index.js'),
		weight: 300,
		meta: {
			label: 'Overview',
			pathname: 'overview',
			icon: 'umb:box-alt',
			workspaces: [workspaceAlias],
		},
	},
	{
		type: 'workspaceEditorView',
		alias: 'Umb.WorkspaceView.Logviewer.Search',
		name: 'LogViewer Root Workspace Search View',
		loader: () => import('../views/search/index.js'),
		weight: 200,
		meta: {
			label: 'Search',
			pathname: 'search',
			icon: 'umb:search',
			workspaces: [workspaceAlias],
		},
	},
];

const workspaceActions: Array<ManifestWorkspaceAction> = [];

const modals: Array<ManifestModal> = [
	{
		type: 'modal',
		alias: 'Umb.Modal.LogViewer.SaveSearch',
		name: 'Saved Searches Modal',
		loader: () => import('../views/search/components/log-viewer-search-input-modal.element.js'),
	},
];

export const manifests = [workspace, ...workspaceViews, ...workspaceActions, ...modals];
