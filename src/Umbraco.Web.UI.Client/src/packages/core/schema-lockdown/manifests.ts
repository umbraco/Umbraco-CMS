import { UMB_SCHEMA_OPERATION_ALLOWED_CONDITION_ALIAS } from './constants.js';
import { UmbSchemaOperationAllowedCondition } from './conditions/schema-operation-allowed.condition.js';
import { UMB_WORKSPACE_ENTITY_IS_NEW_CONDITION_ALIAS } from '@umbraco-cms/backoffice/workspace';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'globalContext',
		alias: 'Umb.GlobalContext.SchemaLockdown',
		name: 'Schema Lockdown Context',
		api: () => import('./schema-lockdown.context.js'),
	},
	{
		type: 'workspaceContext',
		alias: 'Umb.WorkspaceContext.SchemaLockdown',
		name: 'Schema Lockdown Workspace Context',
		api: () => import('./workspace-context/schema-lockdown-workspace.controller.js'),
	},
	{
		type: 'condition',
		name: 'Schema Operation Allowed Condition',
		alias: UMB_SCHEMA_OPERATION_ALLOWED_CONDITION_ALIAS,
		api: UmbSchemaOperationAllowedCondition,
	},
	{
		type: 'workspaceAction',
		alias: 'Umb.WorkspaceAction.SchemaLockdown.ReadOnly',
		name: 'Schema Lockdown Read-only Workspace Action',
		api: () => import('./workspace-action/schema-lockdown-workspace-action.api.js'),
		element: () => import('./workspace-action/schema-lockdown-workspace-action.element.js'),
		weight: 60,
		conditions: [
			{ alias: UMB_WORKSPACE_ENTITY_IS_NEW_CONDITION_ALIAS, match: false },
			{ alias: UMB_SCHEMA_OPERATION_ALLOWED_CONDITION_ALIAS, operation: 'update', match: false },
		],
	},
];
