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

		/**
		 * If true, the condition will skip the start-node check and only check the permissions.
		 * Use this carefully, as this will permit access for Documents that are outside the user's start-node scope.
		 * @default false
		 */
		skipStartNodes?: boolean;
	};

declare global {
	interface UmbExtensionConditionConfigMap {
		UmbDocumentUserPermissionConditionConfig: UmbDocumentUserPermissionConditionConfig;
	}
}
