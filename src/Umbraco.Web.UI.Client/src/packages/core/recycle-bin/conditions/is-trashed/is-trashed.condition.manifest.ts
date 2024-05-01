import { UMB_IS_TRASHED_CONDITION_ALIAS } from './constants.js';
import type { ManifestCondition } from '@umbraco-cms/backoffice/extension-api';

export const manifest: ManifestCondition = {
	type: 'condition',
	name: 'Is trashed Condition',
	alias: UMB_IS_TRASHED_CONDITION_ALIAS,
	api: () => import('./is-trashed.condition.js'),
};
