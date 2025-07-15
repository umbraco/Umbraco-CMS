import { UMB_CURRENT_USER_IS_ADMIN_CONDITION_ALIAS } from './constants.js';
import type { ManifestCondition } from '@umbraco-cms/backoffice/extension-api';

export const manifest: ManifestCondition = {
	type: 'condition',
	name: 'Current user is admin Condition',
	alias: UMB_CURRENT_USER_IS_ADMIN_CONDITION_ALIAS,
	api: () => import('./is-admin.condition.js'),
};
