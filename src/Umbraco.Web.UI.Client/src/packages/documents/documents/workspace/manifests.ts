import {
	UMB_USER_PERMISSION_DOCUMENT_UNPUBLISH,
	UMB_USER_PERMISSION_DOCUMENT_UPDATE,
	UMB_USER_PERMISSION_DOCUMENT_PUBLISH,
} from '../user-permissions/index.js';
import { UMB_DOCUMENT_ENTITY_TYPE } from '../entity.js';
import { UMB_DOCUMENT_WORKSPACE_ALIAS } from './constants.js';
import { UMB_ENTITY_IS_NOT_TRASHED_CONDITION_ALIAS } from '@umbraco-cms/backoffice/recycle-bin';
import { UMB_CONTENT_HAS_PROPERTIES_WORKSPACE_CONDITION } from '@umbraco-cms/backoffice/content';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'workspace',
		kind: 'routable',
		alias: UMB_DOCUMENT_WORKSPACE_ALIAS,
		name: 'Document Workspace',
		api: () => import('./document-workspace.context.js'),
		meta: {
			entityType: UMB_DOCUMENT_ENTITY_TYPE,
		},
	},
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
				match: UMB_DOCUMENT_WORKSPACE_ALIAS,
			},
			{
				alias: 'Umb.Condition.WorkspaceHasCollection',
			},
		],
	},
	{
		type: 'workspaceView',
		kind: 'contentEditor',
		alias: 'Umb.WorkspaceView.Document.Edit',
		name: 'Document Workspace Edit View',
		weight: 200,
		meta: {
			label: '#general_content',
			pathname: 'content',
			icon: 'document',
		},
		conditions: [
			{
				alias: 'Umb.Condition.WorkspaceAlias',
				match: UMB_DOCUMENT_WORKSPACE_ALIAS,
			},
			{
				alias: UMB_CONTENT_HAS_PROPERTIES_WORKSPACE_CONDITION,
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
			label: '#general_info',
			pathname: 'info',
			icon: 'info',
		},
		conditions: [
			{
				alias: 'Umb.Condition.WorkspaceAlias',
				match: UMB_DOCUMENT_WORKSPACE_ALIAS,
			},
		],
	},
	{
		type: 'workspaceAction',
		kind: 'default',
		alias: 'Umb.WorkspaceAction.Document.SaveAndPublish',
		name: 'Save And Publish Document Workspace Action',
		weight: 70,
		api: () => import('./actions/save-and-publish.action.js'),
		meta: {
			label: '#buttons_saveAndPublish',
			look: 'primary',
			color: 'positive',
		},
		conditions: [
			{
				alias: 'Umb.Condition.WorkspaceAlias',
				match: UMB_DOCUMENT_WORKSPACE_ALIAS,
			},
			{
				alias: UMB_ENTITY_IS_NOT_TRASHED_CONDITION_ALIAS,
			},
		],
	},
	{
		type: 'workspaceAction',
		kind: 'default',
		alias: 'Umb.WorkspaceAction.Document.Save',
		name: 'Save Document Workspace Action',
		weight: 80,
		api: () => import('./actions/save.action.js'),
		meta: {
			label: '#buttons_save',
			look: 'secondary',
			color: 'positive',
		},
		conditions: [
			{
				alias: 'Umb.Condition.WorkspaceAlias',
				match: UMB_DOCUMENT_WORKSPACE_ALIAS,
			},
			{
				alias: UMB_ENTITY_IS_NOT_TRASHED_CONDITION_ALIAS,
			},
		],
	},
	{
		type: 'workspaceAction',
		kind: 'default',
		alias: 'Umb.WorkspaceAction.Document.SaveAndPreview',
		name: 'Save And Preview Document Workspace Action',
		weight: 90,
		api: () => import('./actions/save-and-preview.action.js'),
		meta: {
			label: '#buttons_saveAndPreview',
		},
		conditions: [
			{
				alias: 'Umb.Condition.WorkspaceAlias',
				match: UMB_DOCUMENT_WORKSPACE_ALIAS,
			},
			{
				alias: UMB_ENTITY_IS_NOT_TRASHED_CONDITION_ALIAS,
			},
		],
	},
	{
		type: 'workspaceActionMenuItem',
		kind: 'default',
		alias: 'Umb.Document.WorkspaceActionMenuItem.Unpublish',
		name: 'Unpublish',
		weight: 0,
		api: () => import('./actions/unpublish.action.js'),
		forWorkspaceActions: 'Umb.WorkspaceAction.Document.SaveAndPublish',
		meta: {
			label: '#actions_unpublish',
			icon: 'icon-globe',
		},
		conditions: [
			{
				alias: 'Umb.Condition.UserPermission.Document',
				allOf: [UMB_USER_PERMISSION_DOCUMENT_UNPUBLISH],
			},
			{
				alias: UMB_ENTITY_IS_NOT_TRASHED_CONDITION_ALIAS,
			},
		],
	},
	{
		type: 'workspaceActionMenuItem',
		kind: 'default',
		alias: 'Umb.Document.WorkspaceActionMenuItem.PublishWithDescendants',
		name: 'Publish with descendants',
		weight: 10,
		api: () => import('./actions/publish-with-descendants.action.js'),
		forWorkspaceActions: 'Umb.WorkspaceAction.Document.SaveAndPublish',
		meta: {
			label: '#buttons_publishDescendants',
			icon: 'icon-globe',
		},
		conditions: [
			{
				alias: 'Umb.Condition.UserPermission.Document',
				allOf: [UMB_USER_PERMISSION_DOCUMENT_UPDATE, UMB_USER_PERMISSION_DOCUMENT_PUBLISH],
			},
			{
				alias: UMB_ENTITY_IS_NOT_TRASHED_CONDITION_ALIAS,
			},
		],
	},
	{
		type: 'workspaceActionMenuItem',
		kind: 'default',
		alias: 'Umb.Document.WorkspaceActionMenuItem.SchedulePublishing',
		name: 'Schedule publishing',
		weight: 20,
		api: () => import('./actions/save-and-schedule.action.js'),
		forWorkspaceActions: 'Umb.WorkspaceAction.Document.SaveAndPublish',
		meta: {
			label: '#buttons_schedulePublish',
			icon: 'icon-globe',
		},
		conditions: [
			{
				alias: 'Umb.Condition.UserPermission.Document',
				allOf: [UMB_USER_PERMISSION_DOCUMENT_UPDATE, UMB_USER_PERMISSION_DOCUMENT_PUBLISH],
			},
			{
				alias: UMB_ENTITY_IS_NOT_TRASHED_CONDITION_ALIAS,
			},
		],
	},
];
