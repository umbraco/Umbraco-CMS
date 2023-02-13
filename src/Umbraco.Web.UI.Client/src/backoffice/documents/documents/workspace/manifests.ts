import { DOCUMENT_REPOSITORY_ALIAS } from '../repository/manifests';
import { UmbSaveWorkspaceAction } from '../../../shared/workspace-actions/save.action';
import { UmbDocumentSaveAndPublishWorkspaceAction } from './actions/save-and-publish.action';
import { UmbDocumentSaveAndPreviewWorkspaceAction } from './actions/save-and-preview.action';
import { UmbSaveAndScheduleDocumentWorkspaceAction } from './actions/save-and-schedule.action';
import type {
	ManifestWorkspace,
	ManifestWorkspaceAction,
	ManifestWorkspaceView,
	ManifestWorkspaceViewCollection,
} from '@umbraco-cms/models';

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
		loader: () => import('./views/workspace-view-document-edit.element'),
		weight: 200,
		meta: {
			workspaces: ['Umb.Workspace.Document'],
			label: 'Content',
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

const workspaceViewCollections: Array<ManifestWorkspaceViewCollection> = [
	/*
	// TODO: Reenable this:
	{
		type: 'workspaceViewCollection',
		alias: 'Umb.WorkspaceView.Document.Collection',
		name: 'Document Workspace Collection View',
		weight: 300,
		meta: {
			workspaces: ['Umb.Workspace.Document'],
			label: 'Documents',
			pathname: 'collection',
			icon: 'umb:grid',
			entityType: 'document',
			repositoryAlias: DOCUMENT_REPOSITORY_ALIAS,
		},
	},
	*/
];

const workspaceActions: Array<ManifestWorkspaceAction> = [
	{
		type: 'workspaceAction',
		alias: 'Umb.WorkspaceAction.Document.SaveAndPublish',
		name: 'Save And Publish Document Workspace Action',
		weight: 100,
		meta: {
			workspaces: ['Umb.Workspace.Document'],
			label: 'Save And Publish',
			look: 'primary',
			color: 'positive',
			repositoryAlias: DOCUMENT_REPOSITORY_ALIAS,
			api: UmbDocumentSaveAndPublishWorkspaceAction,
		},
	},
	{
		type: 'workspaceAction',
		alias: 'Umb.WorkspaceAction.Document.Save',
		name: 'Save Document Workspace Action',
		weight: 90,
		meta: {
			workspaces: ['Umb.Workspace.Document'],
			label: 'Save',
			look: 'secondary',
			repositoryAlias: DOCUMENT_REPOSITORY_ALIAS,
			api: UmbSaveWorkspaceAction,
		},
	},
	{
		type: 'workspaceAction',
		alias: 'Umb.WorkspaceAction.Document.SaveAndPreview',
		name: 'Save And Preview Document Workspace Action',
		weight: 80,
		meta: {
			workspaces: ['Umb.Workspace.Document'],
			label: 'Save And Preview',
			repositoryAlias: DOCUMENT_REPOSITORY_ALIAS,
			api: UmbDocumentSaveAndPreviewWorkspaceAction,
		},
	},
	{
		type: 'workspaceAction',
		alias: 'Umb.WorkspaceAction.Document.SaveAndSchedule',
		name: 'Save And Schedule Document Workspace Action',
		weight: 70,
		meta: {
			workspaces: ['Umb.Workspace.Document'],
			label: 'Save And Schedule',
			repositoryAlias: DOCUMENT_REPOSITORY_ALIAS,
			api: UmbSaveAndScheduleDocumentWorkspaceAction,
		},
	},
];

export const manifests = [workspace, ...workspaceViews, ...workspaceViewCollections, ...workspaceActions];
