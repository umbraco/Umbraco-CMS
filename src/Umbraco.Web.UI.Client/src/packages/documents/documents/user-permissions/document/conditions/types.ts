import type { UmbConditionConfigBase } from '@umbraco-cms/backoffice/extension-api';

export type UmbDocumentUserPermissionConditionConfig =
	UmbConditionConfigBase<'Umb.Condition.UserPermission.Document'> & {
		/**
		 * The user must have all of the permissions in this array for the condition to be met.
		 * @example
		 * ["Umb.Document.Save", "Umb.Document.Publish"]
		 */
		allOf?: Array<string>;

		/**
		 * The user must have at least one of the permissions in this array for the condition to be met.
		 * @example
		 * ["Umb.Document.Save", "Umb.Document.Publish"]
		 */
		oneOf?: Array<string>;
	};

declare global {
	interface UmbExtensionConditionConfigMap {
		UmbDocumentUserPermissionConditionConfig: UmbDocumentUserPermissionConditionConfig;
	}
}
