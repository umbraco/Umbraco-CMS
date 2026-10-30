import type { UMB_SCHEMA_OPERATION_ALLOWED_CONDITION_ALIAS } from './constants.js';
import type { UmbConditionConfigBase } from '@umbraco-cms/backoffice/extension-api';

export type UmbSchemaOperation = 'create' | 'update' | 'delete';

export interface UmbSchemaOperationAllowedConditionConfig extends UmbConditionConfigBase<
	typeof UMB_SCHEMA_OPERATION_ALLOWED_CONDITION_ALIAS
> {
	/**
	 * The backoffice entity type the action applies to.
	 */
	entityType: string;

	/**
	 * The operation the action performs.
	 */
	operation: UmbSchemaOperation;

	/**
	 * If true (default), the condition is permitted when the operation is allowed.
	 * If false, the condition is permitted when the operation is NOT allowed.
	 */
	match?: boolean;
}

declare global {
	interface UmbExtensionConditionConfigMap {
		umbSchemaOperationAllowedConditionConfig: UmbSchemaOperationAllowedConditionConfig;
	}
}
