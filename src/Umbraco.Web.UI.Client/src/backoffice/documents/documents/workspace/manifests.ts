import type { ManifestWorkspace, ManifestWorkspaceAction, ManifestWorkspaceView } from '@umbraco-cms/models';

const tree: ManifestWorkspace = {
	type: 'workspace',
	alias: 'Umb.Workspace.Document',
	name: 'Document Workspace',
	loader: () => import('./workspace-document.element'),
	meta: {
		entityType: 'document',
	},
};

const workspaceViews: Array<ManifestWorkspaceView> = [
	{
		type: 'workspaceView',
		alias: 'Umb.WorkspaceView.Document.Edit',
		name: 'Document Workspace Edit View',
		loader: () =>
			import('../../../core/components/workspace/workspace-content/views/edit/workspace-view-content-edit.element'),
		weight: 200,
		meta: {
			workspaces: ['Umb.Workspace.Document'],
			label: 'Info',
			pathname: 'content',
			icon: 'document',
		},
	},
	{
		type: 'workspaceView',
		alias: 'Umb.WorkspaceView.Document.Info',
		name: 'Document Workspace Info View',
		loader: () =>
			import('../../../core/components/workspace/workspace-content/views/info/workspace-view-content-info.element'),
		weight: 100,
		meta: {
			workspaces: ['Umb.Workspace.Document'],
			label: 'Info',
			pathname: 'info',
			icon: 'info',
		},
	},
];

const workspaceActions: Array<ManifestWorkspaceAction> = [];

export const manifests = [tree, ...workspaceViews, ...workspaceActions];
