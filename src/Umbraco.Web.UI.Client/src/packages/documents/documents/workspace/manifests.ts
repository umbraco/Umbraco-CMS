import { UMB_DOCUMENT_ENTITY_TYPE } from '../entity.js';
import { UMB_DOCUMENT_WORKSPACE_HAS_COLLECTION_CONDITION } from '../conditions/document-workspace-has-collection.condition.js';
import { UmbUnpublishDocumentEntityAction } from '../entity-actions/unpublish.action.js';
import { UmbDocumentSaveAndPublishWorkspaceAction } from './actions/save-and-publish.action.js';
//import { UmbDocumentSaveAndPreviewWorkspaceAction } from './actions/save-and-preview.action.js';
//import { UmbSaveAndScheduleDocumentWorkspaceAction } from './actions/save-and-schedule.action.js';
import { UmbSaveWorkspaceAction } from '@umbraco-cms/backoffice/workspace';
import type {
	ManifestWorkspace,
	ManifestWorkspaceAction,
	ManifestWorkspaceActionMenuItem,
	ManifestWorkspaceView,
} from '@umbraco-cms/backoffice/extension-registry';

export const UMB_DOCUMENT_WORKSPACE_ALIAS = 'Umb.Workspace.Document';

const workspace: ManifestWorkspace = {
	type: 'workspace',
	alias: UMB_DOCUMENT_WORKSPACE_ALIAS,
	name: 'Document Workspace',
	element: () => import('./document-workspace.element.js'),
	api: () => import('./document-workspace.context.js'),
	meta: {
		entityType: UMB_DOCUMENT_ENTITY_TYPE,
	},
};

const workspaceViews: Array<ManifestWorkspaceView> = [
	{
		type: 'workspaceView',
		alias: 'Umb.WorkspaceView.Document.Collection',
		name: 'Document Workspace Collection View',
		element: () => import('./views/collection/document-workspace-view-collection.element.js'),
		weight: 300,
		meta: {
			label: 'Documents',
			pathname: 'collection',
			icon: 'icon-grid',
		},
		conditions: [
			{
				alias: UMB_DOCUMENT_WORKSPACE_HAS_COLLECTION_CONDITION,
			},
		],
	},
	{
		type: 'workspaceView',
		alias: 'Umb.WorkspaceView.Document.Edit',
		name: 'Document Workspace Edit View',
		element: () => import('./views/edit/document-workspace-view-edit.element.js'),
		weight: 200,
		meta: {
			label: 'Content',
			pathname: 'content',
			icon: 'document',
		},
		conditions: [
			{
				alias: 'Umb.Condition.WorkspaceAlias',
				match: workspace.alias,
			},
		],
	},
	{
		type: 'workspaceView',
		alias: 'Umb.WorkspaceView.Document.Info',
		name: 'Document Workspace Info View',
		element: () => import('./views/info/document-workspace-view-info.element.js'),
		weight: 100,
		meta: {
			label: 'Info',
			pathname: 'info',
			icon: 'info',
		},
		conditions: [
			{
				alias: 'Umb.Condition.WorkspaceAlias',
				match: workspace.alias,
			},
		],
	},
];

const workspaceActions: Array<ManifestWorkspaceAction> = [
	{
		type: 'workspaceAction',
		alias: 'Umb.WorkspaceAction.Document.SaveAndPublish',
		name: 'Save And Publish Document Workspace Action',
		weight: 70,
		api: UmbDocumentSaveAndPublishWorkspaceAction,
		meta: {
			label: 'Save And Publish',
			look: 'primary',
			color: 'positive',
		},
		conditions: [
			{
				alias: 'Umb.Condition.WorkspaceAlias',
				match: workspace.alias,
			},
		],
	},
	{
		type: 'workspaceAction',
		alias: 'Umb.WorkspaceAction.Document.Save',
		name: 'Save Document Workspace Action',
		weight: 80,
		api: UmbSaveWorkspaceAction,
		meta: {
			label: 'Save',
			look: 'secondary',
			color: 'positive',
		},
		conditions: [
			{
				alias: 'Umb.Condition.WorkspaceAlias',
				match: workspace.alias,
			},
		],
	},
	/*
	{
		type: 'workspaceAction',
		alias: 'Umb.WorkspaceAction.Document.SaveAndPreview',
		name: 'Save And Preview Document Workspace Action',
		weight: 90,
		api: UmbDocumentSaveAndPreviewWorkspaceAction,
		meta: {
			label: 'Save And Preview',
		},
		conditions: [
			{
				alias: 'Umb.Condition.WorkspaceAlias',
				match: workspace.alias,
			},
		],
	},
	{
		type: 'workspaceAction',
		alias: 'Umb.WorkspaceAction.Document.SaveAndSchedule',
		name: 'Save And Schedule Document Workspace Action',
		weight: 100,
		api: UmbSaveAndScheduleDocumentWorkspaceAction,
		meta: {
			label: 'Save And Schedule',
		},
		conditions: [
			{
				alias: 'Umb.Condition.WorkspaceAlias',
				match: workspace.alias,
			},
		],
	},
	*/
];

const workspaceActionMenuItems: Array<ManifestWorkspaceActionMenuItem> = [
	{
		type: 'workspaceActionMenuItem',
		alias: 'Umb.WorkspaceActionMenuItem.Unpublish',
		name: 'Unpublish',
		api: UmbUnpublishDocumentEntityAction,
		meta: {
			workspaceActions: ['Umb.WorkspaceAction.Document.SaveAndPublish'],
			label: 'Unpublish',
			repositoryAlias: 'Umb.Repository.Document.Detail',
			entityTypes: [UMB_DOCUMENT_ENTITY_TYPE],
		},
	},
];

export const manifests = [workspace, ...workspaceViews, ...workspaceActions, ...workspaceActionMenuItems];
