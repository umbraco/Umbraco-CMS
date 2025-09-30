import type { UmbConditionConfigBase } from '@umbraco-cms/backoffice/extension-api';

export type UmbDocumentPropertyValueUserPermissionConditionConfig =
	UmbConditionConfigBase<'Umb.Condition.UserPermission.Document.PropertyValue'> & {
		/**
		 * The user must have all of the permissions in this array for the condition to be met.
		 * @example
		 * ["Umb.Document.PropertyValue.Read", "Umb.Document.PropertyValue.Write"]
		 */
		allOf?: Array<string>;

		/**
		 * The user must have at least one of the permissions in this array for the condition to be met.
		 * @example
		 * ["Umb.Document.PropertyValue.Read", "Umb.Document.PropertyValue.Write"]
		 */
		oneOf?: Array<string>;

		match: {
			propertyType: {
				unique: string;
			};
		};
	};

declare global {
	interface UmbExtensionConditionConfigMap {
		umbDocumentPropertyValueUserPermissionConditionConfig: UmbDocumentPropertyValueUserPermissionConditionConfig;
	}
}
