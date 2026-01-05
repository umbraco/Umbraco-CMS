import { UMB_ENTITY_IS_NOT_TRASHED_CONDITION_ALIAS } from './constants.js';
import { UmbEntityIsNotTrashedCondition } from './entity-is-not-trashed.condition.js';
import type { ManifestCondition } from '@umbraco-cms/backoffice/extension-api';

export const manifest: ManifestCondition = {
	type: 'condition',
	name: 'Entity Is not trashed Condition',
	alias: UMB_ENTITY_IS_NOT_TRASHED_CONDITION_ALIAS,
	api: UmbEntityIsNotTrashedCondition,
};
