import type { UMB_CURRENT_USER_GROUP_CONDITION_ALIAS } from './constants.js';
import type { UmbConditionConfigBase } from '@umbraco-cms/backoffice/extension-api';

export interface UmbCurrentUserGroupConditionConfig
	extends UmbConditionConfigBase<typeof UMB_CURRENT_USER_GROUP_CONDITION_ALIAS> {
	/**
	 * The user group(s) that the current user must be a member of to pass the condition.
	 * If set, the current user must be a member of at least one of the specified groups.
	 * If both `grant` and `deny` are set, `grant` takes precedence.
	 * @examples [['guid1', 'guid2'], 'guid3']
	 * @remark The group is identified by its GUID.
	 */
	grant?: string | string[];

	/**
	 * The user group(s) that the current user must not be a member of to pass the condition.
	 * If set, the current user must not be a member of any of the specified groups.
	 * @examples [['guid1', 'guid2'], 'guid3']
	 * @remark The group is identified by its GUID.
	 */
	deny?: string | string[];
}

declare global {
	interface UmbExtensionConditionConfigMap {
		UmbCurrentUserGroupConditionConfig: UmbCurrentUserGroupConditionConfig;
	}
}
