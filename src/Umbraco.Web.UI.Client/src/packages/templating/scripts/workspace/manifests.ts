import { UMB_SCRIPT_ENTITY_TYPE } from '../entity.js';
import {
	UMB_WORKSPACE_CONDITION_ALIAS,
	UmbSchemaLockdownWorkspaceActionApi,
	UmbSchemaLockdownWorkspaceActionElement,
	UmbSubmitWorkspaceAction,
} from '@umbraco-cms/backoffice/workspace';
import { UMB_SCHEMA_OPERATION_ALLOWED_CONDITION_ALIAS } from '@umbraco-cms/backoffice/schema-lockdown';

export const UMB_SCRIPT_WORKSPACE_ALIAS = 'Umb.Workspace.Script';
export const UMB_SAVE_SCRIPT_WORKSPACE_ACTION_ALIAS = 'Umb.WorkspaceAction.Script.Save';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'workspace',
		kind: 'routable',
		alias: UMB_SCRIPT_WORKSPACE_ALIAS,
		name: 'Script Workspace',
		api: () => import('./script-workspace.context.js'),
		meta: {
			entityType: UMB_SCRIPT_ENTITY_TYPE,
		},
	},
	{
		type: 'workspaceAction',
		kind: 'default',
		alias: UMB_SAVE_SCRIPT_WORKSPACE_ACTION_ALIAS,
		name: 'Save Script Workspace Action',
		api: UmbSubmitWorkspaceAction,
		meta: {
			label: '#buttons_save',
			look: 'primary',
			color: 'positive',
		},
		conditions: [
			{
				alias: UMB_WORKSPACE_CONDITION_ALIAS,
				match: UMB_SCRIPT_WORKSPACE_ALIAS,
			},
			{
				alias: UMB_SCHEMA_OPERATION_ALLOWED_CONDITION_ALIAS,
				entityType: UMB_SCRIPT_ENTITY_TYPE,
				operation: 'update',
			},
		],
	},
	{
		type: 'workspaceAction',
		alias: 'Umb.WorkspaceAction.Script.SchemaLockdown',
		name: 'Script Schema Lockdown Workspace Action',
		api: UmbSchemaLockdownWorkspaceActionApi,
		element: UmbSchemaLockdownWorkspaceActionElement,
		conditions: [
			{
				alias: UMB_WORKSPACE_CONDITION_ALIAS,
				match: UMB_SCRIPT_WORKSPACE_ALIAS,
			},
			{
				alias: UMB_SCHEMA_OPERATION_ALLOWED_CONDITION_ALIAS,
				entityType: UMB_SCRIPT_ENTITY_TYPE,
				operation: 'update',
				match: false,
			},
		],
	},
];
