import type { UmbConditionConfigBase } from '@umbraco-cms/backoffice/extension-api';

export type UmbContextualUserPermissionConditionConfig =
	UmbConditionConfigBase<'Umb.Condition.ContextualUserPermission'> & {
		/**
		 * The user must have a permission with this context for the condition to be met.
		 * @example
		 * ["Umbraco.UmbracoNews", "Umbraco.UmbracoNews"]
		 */
		context: string;

		/**
		 * The user must have all of the permissions in this array for the condition to be met.
		 * @example
		 * ["Umb.UmbracoNews.Browse"]
		 */
		allOf?: Array<string>;

		/**
		 * The user must have at least one of the permissions in this array for the condition to be met.
		 * @example
		 * ["Umb.UmbracoNews.Browse"]
		 */
		oneOf?: Array<string>;
	};

declare global {
	interface UmbExtensionConditionConfigMap {
		UmbContextualUserPermissionConditionConfig: UmbContextualUserPermissionConditionConfig;
	}
}
