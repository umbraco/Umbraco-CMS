import { MEDIA_REPOSITORY_ALIAS } from '../repository/manifests';
import { UmbSaveWorkspaceAction } from '@umbraco-cms/backoffice/workspace';
import type {
	ManifestWorkspace,
	ManifestWorkspaceAction,
	ManifestWorkspaceView,
	ManifestWorkspaceViewCollection,
} from '@umbraco-cms/backoffice/extensions-registry';

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
		loader: () => import('./views/edit/media-edit-workspace-view.element'),
		weight: 200,
		meta: {
			label: 'Media',
			pathname: 'media',
			icon: 'umb:picture',
		},
		conditions: {
			workspaces: ['Umb.Workspace.Media'],
		},
	},
	{
		type: 'workspaceView',
		alias: 'Umb.WorkspaceView.Media.Info',
		name: 'Media Workspace Info View',
		loader: () => import('./views/info/media-info-workspace-view.element'),
		weight: 100,
		meta: {
			label: 'Info',
			pathname: 'info',
			icon: 'info',
		},
		conditions: {
			workspaces: ['Umb.Workspace.Media'],
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
			label: 'Media',
			pathname: 'collection',
			icon: 'umb:grid',
			entityType: 'media',
			repositoryAlias: MEDIA_REPOSITORY_ALIAS,
		},
		conditions: {
			workspaces: ['Umb.Workspace.Media'],
		},
	},
];

const workspaceActions: Array<ManifestWorkspaceAction> = [
	{
		type: 'workspaceAction',
		alias: 'Umb.WorkspaceAction.Media.Save',
		name: 'Save Media Workspace Action',
		meta: {
			label: 'Save',
			look: 'primary',
			color: 'positive',
			api: UmbSaveWorkspaceAction,
		},
		conditions: {
			workspaces: ['Umb.Workspace.Media'],
		},
	},
];

export const manifests = [workspace, ...workspaceViews, ...workspaceViewCollections, ...workspaceActions];
