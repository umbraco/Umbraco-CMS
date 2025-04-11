import type { UmbConditionConfigBase } from '@umbraco-cms/backoffice/extension-api';

export type UmbLanguageUserPermissionConditionConfig =
	UmbConditionConfigBase<'Umb.Condition.UserPermission.Language'> & {
		/**
		 * The user must have all of the permissions in this array for the condition to be met.
		 * @example
		 * ["en", "da"]
		 */
		allOf?: Array<string>;

		/**
		 * The user must have at least one of the permissions in this array for the condition to be met.
		 * @example
		 * ["en", "da"]
		 */
		oneOf?: Array<string>;

		match: string;
	};

declare global {
	interface UmbExtensionConditionConfigMap {
		umbLanguageUserPermissionConditionConfig: UmbLanguageUserPermissionConditionConfig;
	}
}
