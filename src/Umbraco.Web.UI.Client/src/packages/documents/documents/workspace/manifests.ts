import { UMB_DOCUMENT_ENTITY_TYPE } from '../entity.js';
import { UmbDocumentSaveAndScheduleWorkspaceAction } from './actions/save-and-schedule.action.js';
import { UmbDocumentUnpublishWorkspaceAction } from './actions/unpublish.action.js';
import { UmbDocumentSaveAndPublishWorkspaceAction } from './actions/save-and-publish.action.js';
import { UmbDocumentPublishWithDescendantsWorkspaceAction } from './actions/publish-with-descendants.action.js';
import { UmbSaveWorkspaceAction } from '@umbraco-cms/backoffice/workspace';
import type {
	ManifestWorkspaces,
	ManifestWorkspaceActions,
	ManifestWorkspaceActionMenuItem,
	ManifestWorkspaceView,
} from '@umbraco-cms/backoffice/extension-registry';

export const UMB_DOCUMENT_WORKSPACE_ALIAS = 'Umb.Workspace.Document';

const workspace: ManifestWorkspaces = {
	type: 'workspace',
	kind: 'routable',
	alias: UMB_DOCUMENT_WORKSPACE_ALIAS,
	name: 'Document Workspace',
	api: () => import('./document-workspace.context.js'),
	meta: {
		entityType: UMB_DOCUMENT_ENTITY_TYPE,
	},
};

const workspaceViews: Array<ManifestWorkspaceView> = [
	{
		type: 'workspaceView',
		kind: 'collection',
		alias: 'Umb.WorkspaceView.Document.Collection',
		name: 'Document Workspace Collection View',
		meta: {
			label: 'Collection',
			pathname: 'collection',
			icon: 'icon-grid',
		},
		conditions: [
			{
				alias: 'Umb.Condition.WorkspaceAlias',
				match: workspace.alias,
			},
			{
				alias: 'Umb.Condition.WorkspaceHasCollection',
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

const workspaceActions: Array<ManifestWorkspaceActions> = [
	{
		type: 'workspaceAction',
		kind: 'default',
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
		kind: 'default',
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
	*/
];

const workspaceActionMenuItems: Array<ManifestWorkspaceActionMenuItem> = [
	{
		type: 'workspaceActionMenuItem',
		kind: 'default',
		alias: 'Umb.Document.WorkspaceActionMenuItem.Unpublish',
		name: 'Unpublish',
		weight: 0,
		api: UmbDocumentUnpublishWorkspaceAction,
		forWorkspaceActions: 'Umb.WorkspaceAction.Document.SaveAndPublish',
		meta: {
			label: 'Unpublish...',
			icon: 'icon-globe',
		},
	},
	{
		type: 'workspaceActionMenuItem',
		kind: 'default',
		alias: 'Umb.Document.WorkspaceActionMenuItem.PublishWithDescendants',
		name: 'Publish with descendants',
		weight: 10,
		api: UmbDocumentPublishWithDescendantsWorkspaceAction,
		forWorkspaceActions: 'Umb.WorkspaceAction.Document.SaveAndPublish',
		meta: {
			label: 'Publish with descendants...',
			icon: 'icon-globe',
		},
	},
	{
		type: 'workspaceActionMenuItem',
		kind: 'default',
		alias: 'Umb.Document.WorkspaceActionMenuItem.SchedulePublishing',
		name: 'Schedule publishing',
		weight: 20,
		api: UmbDocumentSaveAndScheduleWorkspaceAction,
		forWorkspaceActions: 'Umb.WorkspaceAction.Document.SaveAndPublish',
		meta: {
			label: 'Schedule...',
			icon: 'icon-globe',
		},
	},
];

export const manifests = [workspace, ...workspaceViews, ...workspaceActions, ...workspaceActionMenuItems];
