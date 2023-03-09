import { MEDIA_REPOSITORY_ALIAS } from '../repository/manifests';
import { UmbSaveWorkspaceAction } from '@umbraco-cms/workspace';
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
			repositoryAlias: MEDIA_REPOSITORY_ALIAS,
		},
	},
];

const workspaceActions: Array<ManifestWorkspaceAction> = [
	{
		type: 'workspaceAction',
		alias: 'Umb.WorkspaceAction.Media.Save',
		name: 'Save Media Workspace Action',
		meta: {
			workspaces: ['Umb.Workspace.Media'],
			label: 'Save',
			look: 'primary',
			color: 'positive',
			api: UmbSaveWorkspaceAction,
		},
	},
];

export const manifests = [workspace, ...workspaceViews, ...workspaceViewCollections, ...workspaceActions];
