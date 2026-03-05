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

declare global {
	interface UmbExtensionConditionConfigMap {
		UmbElementUserPermissionConditionConfig: UmbElementUserPermissionConditionConfig;
	}
}
