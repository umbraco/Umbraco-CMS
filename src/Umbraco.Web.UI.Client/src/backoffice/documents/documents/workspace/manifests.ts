import type { ManifestWorkspace, ManifestWorkspaceAction, ManifestWorkspaceView } from '@umbraco-cms/models';

const workspace: ManifestWorkspace = {
	type: 'workspace',
	alias: 'Umb.Workspace.Document',
	name: 'Document Workspace',
	loader: () => import('./document-workspace.element'),
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
			import('../../../shared/components/workspace/workspace-content/views/edit/workspace-view-content-edit.element'),
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
			import('../../../shared/components/workspace/workspace-content/views/info/workspace-view-content-info.element'),
		weight: 100,
		meta: {
			workspaces: ['Umb.Workspace.Document'],
			label: 'Info',
			pathname: 'info',
			icon: 'info',
		},
	},
];

const workspaceActions: Array<ManifestWorkspaceAction> = [
	{
		type: 'workspaceAction',
		alias: 'Umb.WorkspaceAction.Document.SaveAndPreview',
		name: 'Save Document Workspace Action',
		loader: () => import('src/backoffice/shared/components/workspace/actions/save/workspace-action-node-save.element'),
		meta: {
			workspaces: ['Umb.Workspace.Document'],
			label: 'Save and preview',
		},
	},
	{
		type: 'workspaceAction',
		alias: 'Umb.WorkspaceAction.Document.Save',
		name: 'Save Document Workspace Action',
		loader: () => import('src/backoffice/shared/components/workspace/actions/save/workspace-action-node-save.element'),
		meta: {
			workspaces: ['Umb.Workspace.Document'],
			look: 'secondary',
			label: 'Save',
		},
	},
	{
		type: 'workspaceAction',
		alias: 'Umb.WorkspaceAction.Document.SaveAndPublish',
		name: 'Save Document Workspace Action',
		loader: () => import('src/backoffice/shared/components/workspace/actions/save/workspace-action-node-save.element'),
		meta: {
			workspaces: ['Umb.Workspace.Document'],
			label: 'Save and publish',
			look: 'primary',
			color: 'positive',
		},
	},
];

export const manifests = [workspace, ...workspaceViews, ...workspaceActions];
