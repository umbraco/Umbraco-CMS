import type { UMB_SCHEMA_OPERATION_ALLOWED_CONDITION_ALIAS } from './constants.js';
import type { UmbConditionConfigBase } from '@umbraco-cms/backoffice/extension-api';

/**
 * A schema operation that schema lockdown can deny. Reading is never denied.
 */
export type UmbSchemaOperation = 'create' | 'update' | 'delete';

/**
 * Configuration of the {@link UMB_SCHEMA_OPERATION_ALLOWED_CONDITION_ALIAS} condition.
 */
export interface UmbSchemaOperationAllowedConditionConfig extends UmbConditionConfigBase<
	typeof UMB_SCHEMA_OPERATION_ALLOWED_CONDITION_ALIAS
> {
	/**
	 * The backoffice entity type the action applies to. Defaults to the entity type of the surrounding workspace.
	 */
	entityType?: string;

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
