import type { UMB_SECTION_USER_NO_PERMISSION_CONDITION_ALIAS } from './constants.js';
import type { UmbConditionConfigBase } from '@umbraco-cms/backoffice/extension-api';

export type UmbSectionUserNoPermissionConditionConfig = UmbConditionConfigBase<
	typeof UMB_SECTION_USER_NO_PERMISSION_CONDITION_ALIAS
> & {
	/**
	 * The section alias to check that the user does NOT have access to.
	 * @example
	 * "Umb.Section.Users"
	 */
	match: string;
};

declare global {
	interface UmbExtensionConditionConfigMap {
		UmbSectionUserNoPermissionConditionConfig: UmbSectionUserNoPermissionConditionConfig;
	}
}
