import { UMB_ENTITY_HAS_CHILDREN_CONDITION_ALIAS } from './constants.js';
import { UmbEntityHasChildrenCondition } from './entity-has-children.condition.js';
import type { ManifestCondition } from '@umbraco-cms/backoffice/extension-api';

export const manifest: ManifestCondition = {
	type: 'condition',
	name: 'Entity Has Children Condition',
	alias: UMB_ENTITY_HAS_CHILDREN_CONDITION_ALIAS,
	api: UmbEntityHasChildrenCondition,
};
