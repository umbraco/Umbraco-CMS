import { UMB_DATA_TYPE_ENTITY_TYPE } from '../entity.js';
import { UMB_DATA_TYPE_WORKSPACE_ALIAS } from './constants.js';
import {
	UMB_WORKSPACE_CONDITION_ALIAS,
	UmbSchemaLockdownInfoAppElement,
	UmbSchemaLockdownWorkspaceActionApi,
	UmbSchemaLockdownWorkspaceActionElement,
	UmbSubmitWorkspaceAction,
} from '@umbraco-cms/backoffice/workspace';
import { UMB_SCHEMA_OPERATION_ALLOWED_CONDITION_ALIAS } from '@umbraco-cms/backoffice/schema-lockdown';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'workspace',
		kind: 'routable',
		alias: UMB_DATA_TYPE_WORKSPACE_ALIAS,
		name: 'Data Type Workspace',
		api: () => import('./data-type-workspace.context.js'),
		meta: {
			entityType: 'data-type',
		},
	},
	{
		type: 'workspaceView',
		alias: 'Umb.WorkspaceView.DataType.Edit',
		name: 'Data Type Workspace Edit View',
		element: () => import('./views/details/data-type-details-workspace-view.element.js'),
		weight: 90,
		meta: {
			label: '#general_details',
			pathname: 'details',
			icon: 'edit',
		},
		conditions: [
			{
				alias: UMB_WORKSPACE_CONDITION_ALIAS,
				match: UMB_DATA_TYPE_WORKSPACE_ALIAS,
			},
		],
	},
	{
		type: 'workspaceView',
		alias: 'Umb.WorkspaceView.DataType.Info',
		name: 'Data Type Workspace Info View',
		element: () => import('./views/info/workspace-view-data-type-info.element.js'),
		weight: 90,
		meta: {
			label: '#general_info',
			pathname: 'info',
			icon: 'info',
		},
		conditions: [
			{
				alias: UMB_WORKSPACE_CONDITION_ALIAS,
				match: UMB_DATA_TYPE_WORKSPACE_ALIAS,
			},
		],
	},
	{
		type: 'workspaceAction',
		kind: 'default',
		alias: 'Umb.WorkspaceAction.DataType.Save',
		name: 'Save Data Type Workspace Action',
		api: UmbSubmitWorkspaceAction,
		meta: {
			label: '#buttons_save',
			look: 'primary',
			color: 'positive',
		},
		conditions: [
			{
				alias: UMB_WORKSPACE_CONDITION_ALIAS,
				match: UMB_DATA_TYPE_WORKSPACE_ALIAS,
			},
			{
				alias: UMB_SCHEMA_OPERATION_ALLOWED_CONDITION_ALIAS,
				entityType: UMB_DATA_TYPE_ENTITY_TYPE,
				operation: 'update',
			},
		],
	},
	{
		type: 'workspaceAction',
		alias: 'Umb.WorkspaceAction.DataType.SchemaLockdown',
		name: 'Data Type Schema Lockdown Workspace Action',
		api: UmbSchemaLockdownWorkspaceActionApi,
		element: UmbSchemaLockdownWorkspaceActionElement,
		conditions: [
			{
				alias: UMB_WORKSPACE_CONDITION_ALIAS,
				match: UMB_DATA_TYPE_WORKSPACE_ALIAS,
			},
			{
				alias: UMB_SCHEMA_OPERATION_ALLOWED_CONDITION_ALIAS,
				entityType: UMB_DATA_TYPE_ENTITY_TYPE,
				operation: 'update',
				match: false,
			},
		],
	},
	{
		type: 'workspaceInfoApp',
		alias: 'Umb.WorkspaceInfoApp.DataType.SchemaLockdown',
		name: 'Data Type Schema Lockdown Notice',
		element: UmbSchemaLockdownInfoAppElement,
		weight: 1000,
		conditions: [
			{
				alias: UMB_WORKSPACE_CONDITION_ALIAS,
				match: UMB_DATA_TYPE_WORKSPACE_ALIAS,
			},
			{
				alias: UMB_SCHEMA_OPERATION_ALLOWED_CONDITION_ALIAS,
				entityType: UMB_DATA_TYPE_ENTITY_TYPE,
				operation: 'update',
				match: false,
			},
		],
	},
];
