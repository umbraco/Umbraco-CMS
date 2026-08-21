import { UMB_DATA_TYPE_ENTITY_TYPE, UMB_DATA_TYPE_FOLDER_ENTITY_TYPE } from '../../../entity.js';
import { UMB_DATA_TYPE_FOLDER_WORKSPACE_ALIAS } from './constants.js';
import {
	UMB_WORKSPACE_CONDITION_ALIAS,
	UmbSchemaLockdownWorkspaceActionApi,
	UmbSchemaLockdownWorkspaceActionElement,
	UmbSubmitWorkspaceAction,
} from '@umbraco-cms/backoffice/workspace';
import { UMB_SCHEMA_OPERATION_ALLOWED_CONDITION_ALIAS } from '@umbraco-cms/backoffice/schema-lockdown';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'workspace',
		kind: 'routable',
		alias: UMB_DATA_TYPE_FOLDER_WORKSPACE_ALIAS,
		name: 'Data Type Folder Workspace',
		api: () => import('./data-type-folder-workspace.context.js'),
		meta: {
			entityType: UMB_DATA_TYPE_FOLDER_ENTITY_TYPE,
		},
	},
	{
		type: 'workspaceAction',
		kind: 'default',
		alias: 'Umb.WorkspaceAction.DataType.Folder.Submit',
		name: 'Submit Media Type Folder Workspace Action',
		api: UmbSubmitWorkspaceAction,
		meta: {
			label: '#buttons_save',
			look: 'primary',
			color: 'positive',
		},
		conditions: [
			{
				alias: UMB_WORKSPACE_CONDITION_ALIAS,
				match: UMB_DATA_TYPE_FOLDER_WORKSPACE_ALIAS,
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
		alias: 'Umb.WorkspaceAction.DataType.Folder.SchemaLockdown',
		name: 'Data Type Folder Schema Lockdown Workspace Action',
		api: UmbSchemaLockdownWorkspaceActionApi,
		element: UmbSchemaLockdownWorkspaceActionElement,
		conditions: [
			{
				alias: UMB_WORKSPACE_CONDITION_ALIAS,
				match: UMB_DATA_TYPE_FOLDER_WORKSPACE_ALIAS,
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
