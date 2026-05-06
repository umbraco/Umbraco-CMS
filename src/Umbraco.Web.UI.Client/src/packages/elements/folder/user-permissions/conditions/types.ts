import type { UmbConditionConfigBase } from '@umbraco-cms/backoffice/extension-api';

export type UmbElementFolderUserPermissionConditionConfig =
	UmbConditionConfigBase<'Umb.Condition.UserPermission.ElementFolder'> & {
		/**
		 * The user must have all of the permissions in this array for the condition to be met.
		 * @example
		 * ["Umb.ElementContainer.Create", "Umb.ElementContainer.Read"]
		 */
		allOf?: Array<string>;

		/**
		 * The user must have at least one of the permissions in this array for the condition to be met.
		 * @example
		 * ["Umb.ElementContainer.Create", "Umb.ElementContainer.Read"]
		 */
		oneOf?: Array<string>;
	};

declare global {
	interface UmbExtensionConditionConfigMap {
		UmbElementFolderUserPermissionConditionConfig: UmbElementFolderUserPermissionConditionConfig;
	}
}
