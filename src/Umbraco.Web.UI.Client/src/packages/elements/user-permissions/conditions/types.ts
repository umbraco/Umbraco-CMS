import type { UmbConditionConfigBase } from '@umbraco-cms/backoffice/extension-api';

export type UmbElementUserPermissionConditionConfig = UmbConditionConfigBase<'Umb.Condition.UserPermission.Element'> & {
	/**
	 * The user must have all of the permissions in this array for the condition to be met.
	 * @example
	 * ["Umb.Element.Save", "Umb.Element.Publish"]
	 */
	allOf?: Array<string>;

	/**
	 * The user must have at least one of the permissions in this array for the condition to be met.
	 * @example
	 * ["Umb.Element.Save", "Umb.Element.Publish"]
	 */
	oneOf?: Array<string>;
};

export type UmbPermissionVerbsConfig = {
	allOf?: Array<string>;
	oneOf?: Array<string>;
};

export type UmbElementOrElementFolderUserPermissionConditionConfig =
	UmbConditionConfigBase<'Umb.Condition.UserPermission.ElementOrElementFolder'> & {
		/**
		 * Permission verbs to check against element permissions.
		 * The condition is met if the user has the required element permissions OR the required folder permissions.
		 * @example
		 * { allOf: ["Umb.Element.Read"] }
		 */
		element?: UmbPermissionVerbsConfig;

		/**
		 * Permission verbs to check against element folder (container) permissions.
		 * The condition is met if the user has the required element permissions OR the required folder permissions.
		 * @example
		 * { allOf: ["Umb.ElementContainer.Read"] }
		 */
		folder?: UmbPermissionVerbsConfig;
	};

declare global {
	interface UmbExtensionConditionConfigMap {
		UmbElementUserPermissionConditionConfig: UmbElementUserPermissionConditionConfig;
		UmbElementOrElementFolderUserPermissionConditionConfig: UmbElementOrElementFolderUserPermissionConditionConfig;
	}
}
