import { UMB_CURRENT_USER_GROUP_CONDITION_ALIAS } from './constants.js';
import type { ManifestCondition } from '@umbraco-cms/backoffice/extension-api';

export const manifest: ManifestCondition = {
	type: 'condition',
	name: 'Current user group Condition',
	alias: UMB_CURRENT_USER_GROUP_CONDITION_ALIAS,
	api: () => import('./group.condition.js'),
};
