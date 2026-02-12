import { UMB_CURRENT_USER_ALLOW_MFA_CONDITION_ALIAS } from './constants.js';
import type { ManifestCondition } from '@umbraco-cms/backoffice/extension-api';

export const manifest: ManifestCondition = {
	type: 'condition',
	name: 'Current User Allow Mfa Action Condition',
	alias: UMB_CURRENT_USER_ALLOW_MFA_CONDITION_ALIAS,
	api: () => import('./current-user-allow-mfa-action.condition.js'),
};
