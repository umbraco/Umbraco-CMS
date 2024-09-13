import type { UmbConditionConfigBase } from '@umbraco-cms/backoffice/extension-api';

export type UmbSectionUserPermissionConditionConfig = UmbConditionConfigBase<'Umb.Condition.SectionUserPermission'> & {
	/**
	 *
	 *
	 * @example
	 * "Umb.Section.Content"
	 */
	match: string;
};

declare global {
	interface UmbExtensionConditionMap {
		UmbSectionUserPermissionConditionConfig: UmbSectionUserPermissionConditionConfig;
	}
}
