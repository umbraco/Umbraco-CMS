import { UMB_IS_NOT_TRASHED_CONDITION_ALIAS } from './constants.js';
import type { ManifestCondition } from '@umbraco-cms/backoffice/extension-api';

export const manifest: ManifestCondition = {
	type: 'condition',
	name: 'Is not trashed Condition',
	alias: UMB_IS_NOT_TRASHED_CONDITION_ALIAS,
	api: () => import('./is-not-trashed.condition.js'),
};
