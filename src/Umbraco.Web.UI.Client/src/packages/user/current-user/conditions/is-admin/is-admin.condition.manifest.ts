import { UMB_CURRENT_USER_IS_ADMIN_CONDITION_ALIAS } from './constants.js';
import { UmbCurrentUserIsAdminCondition } from './is-admin.condition.js';
import type { ManifestCondition } from '@umbraco-cms/backoffice/extension-api';

export const manifest: ManifestCondition = {
	type: 'condition',
	name: 'Current user is admin Condition',
	alias: UMB_CURRENT_USER_IS_ADMIN_CONDITION_ALIAS,
	api: UmbCurrentUserIsAdminCondition,
};
