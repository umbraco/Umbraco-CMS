import { UmbDocumentSaveAndPublishWorkspaceAction } from './actions/save-and-publish.action';
import { UmbDocumentSaveAndPreviewWorkspaceAction } from './actions/save-and-preview.action';
import { UmbSaveAndScheduleDocumentWorkspaceAction } from './actions/save-and-schedule.action';
import { UmbSaveWorkspaceAction } from '@umbraco-cms/backoffice/workspace';
import type {
	ManifestWorkspace,
	ManifestWorkspaceAction,
	ManifestWorkspaceView,
	ManifestWorkspaceViewCollection,
} from '@umbraco-cms/backoffice/extensions-registry';

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
		loader: () => import('./views/edit/document-workspace-view-edit.element'),
		weight: 200,
		meta: {
			label: 'Content',
			pathname: 'content',
			icon: 'document',
		},
		conditions: {
			workspaces: ['Umb.Workspace.Document'],
		},
	},
	{
		type: 'workspaceView',
		alias: 'Umb.WorkspaceView.Document.Info',
		name: 'Document Workspace Info View',
		loader: () => import('./views/info/document-info-workspace-view.element'),
		weight: 100,
		meta: {
			label: 'Info',
			pathname: 'info',
			icon: 'info',
		},
		conditions: {
			workspaces: ['Umb.Workspace.Document'],
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
			label: 'Documents',
			pathname: 'collection',
			icon: 'umb:grid',
			entityType: 'document',
			repositoryAlias: DOCUMENT_REPOSITORY_ALIAS,
		},
		conditions: {
			workspaces: ['Umb.Workspace.Document'],
		}
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
			label: 'Save And Publish',
			look: 'primary',
			color: 'positive',
			api: UmbDocumentSaveAndPublishWorkspaceAction,
		},
		conditions: {
			workspaces: ['Umb.Workspace.Document'],
		},
	},
	{
		type: 'workspaceAction',
		alias: 'Umb.WorkspaceAction.Document.Save',
		name: 'Save Document Workspace Action',
		weight: 90,
		meta: {
			label: 'Save',
			look: 'secondary',
			api: UmbSaveWorkspaceAction,
		},
		conditions: {
			workspaces: ['Umb.Workspace.Document'],
		},
	},
	{
		type: 'workspaceAction',
		alias: 'Umb.WorkspaceAction.Document.SaveAndPreview',
		name: 'Save And Preview Document Workspace Action',
		weight: 80,
		meta: {
			label: 'Save And Preview',
			api: UmbDocumentSaveAndPreviewWorkspaceAction,
		},
		conditions: {
			workspaces: ['Umb.Workspace.Document'],
		},
	},
	{
		type: 'workspaceAction',
		alias: 'Umb.WorkspaceAction.Document.SaveAndSchedule',
		name: 'Save And Schedule Document Workspace Action',
		weight: 70,
		meta: {
			label: 'Save And Schedule',
			api: UmbSaveAndScheduleDocumentWorkspaceAction,
		},
		conditions: {
			workspaces: ['Umb.Workspace.Document'],
		},
	},
];

export const manifests = [workspace, ...workspaceViews, ...workspaceViewCollections, ...workspaceActions];
