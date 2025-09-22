import type { UmbConditionConfigBase } from '@umbraco-cms/backoffice/extension-api';

export type UmbFallbackUserPermissionConditionConfig =
	UmbConditionConfigBase<'Umb.Condition.UserPermission.Fallback'> & {
		/**
		 * The user must have all of the permissions in this array for the condition to be met.
		 * @example
		 * ["Umb.PermissionOne", "Umb.PermissionTwo"]
		 */
		allOf?: Array<string>;

		/**
		 * The user must have at least one of the permissions in this array for the condition to be met.
		 * @example
		 * ["Umb.PermissionOne", "Umb.PermissionTwo"]
		 */
		oneOf?: Array<string>;
	};

declare global {
	interface UmbExtensionConditionConfigMap {
		UmbFallbackUserPermissionConditionConfig: UmbFallbackUserPermissionConditionConfig;
	}
}
