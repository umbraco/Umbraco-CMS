import { UMB_ENTITY_IS_TRASHED_CONDITION_ALIAS } from './constants.js';
import { UmbEntityIsTrashedCondition } from './entity-is-trashed.condition.js';
import type { ManifestCondition } from '@umbraco-cms/backoffice/extension-api';

export const manifest: ManifestCondition = {
	type: 'condition',
	name: 'Entity Is trashed Condition',
	alias: UMB_ENTITY_IS_TRASHED_CONDITION_ALIAS,
	api: UmbEntityIsTrashedCondition,
};
