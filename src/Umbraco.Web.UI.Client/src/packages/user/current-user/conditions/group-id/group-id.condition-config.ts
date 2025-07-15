import type { UMB_CURRENT_USER_GROUP_ID_CONDITION_ALIAS } from './constants.js';
import type { UmbConditionConfigBase } from '@umbraco-cms/backoffice/extension-api';

export interface UmbCurrentUserGroupIdConditionConfig
	extends UmbConditionConfigBase<typeof UMB_CURRENT_USER_GROUP_ID_CONDITION_ALIAS> {
	/**
	 * The user group that the current user must be a member of to pass the condition.
	 * @examples ['guid1']
	 */
	match?: string;

	/**
	 * The user group(s) that the current user must be a member of to pass the condition.
	 * @examples [['guid1', 'guid2']]
	 */
	oneOf?: Array<string>;

	/**
	 * The user groups that the current user must be a member of to pass the condition.
	 * @examples [['guid1', 'guid2']]
	 */
	allOf?: Array<string>;

	/**
	 * The user group(s) that the current user must not be a member of to pass the condition.
	 * @examples [['guid1', 'guid2']]
	 */
	noneOf?: Array<string>;
}

declare global {
	interface UmbExtensionConditionConfigMap {
		UmbCurrentUserGroupIdConditionConfig: UmbCurrentUserGroupIdConditionConfig;
	}
}
