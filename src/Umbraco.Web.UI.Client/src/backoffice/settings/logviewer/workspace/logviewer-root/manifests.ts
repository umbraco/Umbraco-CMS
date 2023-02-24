import type { ManifestWorkspace, ManifestWorkspaceAction, ManifestWorkspaceView } from '@umbraco-cms/models';

const workspaceAlias = 'Umb.Workspace.LogviewerRoot';

const workspace: ManifestWorkspace = {
	type: 'workspace',
	alias: workspaceAlias,
	name: 'LogViewer Root Workspace',
	loader: () => import('./logviewer-root-workspace.element'),
	meta: {
		entityType: 'logviewer',
	},
};

const workspaceViews: Array<ManifestWorkspaceView> = [
	{
		type: 'workspaceView',
		alias: 'Umb.WorkspaceView.Logviewer.Overview',
		name: 'LogViewer Root Workspace Overview View',
		loader: () => import('../views/log-overview.element'),
		weight: 300,
		meta: {
			workspaces: [workspaceAlias],
			label: '',
			pathname: 'overview',
			icon: '',
		},
	},
	{
		type: 'workspaceView',
		alias: 'Umb.WorkspaceView.Logviewer.Search',
		name: 'LogViewer Root Workspace Search View',
		loader: () => import('../views/log-search.element'),
		weight: 200,
		meta: {
			workspaces: [workspaceAlias],
			label: '',
			pathname: 'search',
			icon: '',
		},
	},
];

const workspaceActions: Array<ManifestWorkspaceAction> = [];

export const manifests = [workspace, ...workspaceViews, ...workspaceActions];
