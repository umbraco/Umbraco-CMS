import type {
	ManifestWorkspace,
	ManifestWorkspaceAction,
	ManifestWorkspaceView,
	ManifestWorkspaceViewCollection,
} from '@umbraco-cms/models';

const workspace: ManifestWorkspace = {
	type: 'workspace',
	alias: 'Umb.Workspace.Media',
	name: 'Media Workspace',
	loader: () => import('./media-workspace.element'),
	meta: {
		entityType: 'media',
	},
};

const workspaceViews: Array<ManifestWorkspaceView> = [
	{
		type: 'workspaceView',
		alias: 'Umb.WorkspaceView.Media.Edit',
		name: 'Media Workspace Edit View',
		loader: () =>
			import('../../../shared/components/workspace/workspace-content/views/edit/workspace-view-content-edit.element'),
		weight: 200,
		meta: {
			workspaces: ['Umb.Workspace.Media'],
			label: 'Media',
			pathname: 'media',
			icon: 'umb:picture',
		},
	},
	{
		type: 'workspaceView',
		alias: 'Umb.WorkspaceView.Media.Info',
		name: 'Media Workspace Info View',
		loader: () =>
			import('../../../shared/components/workspace/workspace-content/views/info/workspace-view-content-info.element'),
		weight: 100,
		meta: {
			workspaces: ['Umb.Workspace.Media'],
			label: 'Info',
			pathname: 'info',
			icon: 'info',
		},
	},
];

const workspaceViewCollections: Array<ManifestWorkspaceViewCollection> = [
	{
		type: 'workspaceViewCollection',
		alias: 'Umb.WorkspaceView.Media.Collection',
		name: 'Media Workspace Collection View',
		weight: 300,
		meta: {
			workspaces: ['Umb.Workspace.Media'],
			label: 'Media',
			pathname: 'collection',
			icon: 'umb:grid',
			entityType: 'media',
			storeAlias: 'umbMediaStore',
		},
	},
];

const workspaceActions: Array<ManifestWorkspaceAction> = [
	{
		type: 'workspaceAction',
		alias: 'Umb.WorkspaceAction.Media.Save',
		name: 'Save Media Workspace Action',
		loader: () => import('src/backoffice/shared/components/workspace/actions/save/workspace-action-node-save.element'),
		meta: {
			workspaces: ['Umb.Workspace.Media'],
			look: 'primary',
			color: 'positive',
		},
	},
];

export const manifests = [workspace, ...workspaceViews, ...workspaceViewCollections, ...workspaceActions];
