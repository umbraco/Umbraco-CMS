import { UMB_CURRENT_USER_GROUP_ID_CONDITION_ALIAS } from './constants.js';
import { UmbCurrentUserGroupCondition } from './group-id.condition.js';
import type { ManifestCondition } from '@umbraco-cms/backoffice/extension-api';

export const manifest: ManifestCondition = {
	type: 'condition',
	name: 'Current user group id Condition',
	alias: UMB_CURRENT_USER_GROUP_ID_CONDITION_ALIAS,
	api: UmbCurrentUserGroupCondition,
};
