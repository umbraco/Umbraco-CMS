import type { UMB_SECTION_USER_PERMISSION_CONDITION_ALIAS } from './constants.js';
import type { UmbConditionConfigBase } from '@umbraco-cms/backoffice/extension-api';

export type UmbSectionUserPermissionConditionConfig = UmbConditionConfigBase<
	typeof UMB_SECTION_USER_PERMISSION_CONDITION_ALIAS
> & {
	/**
	 *
	 *
	 * @example
	 * "Umb.Section.Content"
	 */
	match: string;
};

declare global {
	interface UmbExtensionConditionConfigMap {
		UmbSectionUserPermissionConditionConfig: UmbSectionUserPermissionConditionConfig;
	}
}
