import { UMB_SCHEMA_OPERATION_ALLOWED_CONDITION_ALIAS } from './constants.js';
import { UmbSchemaOperationAllowedCondition } from './conditions/schema-operation-allowed.condition.js';

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
];
