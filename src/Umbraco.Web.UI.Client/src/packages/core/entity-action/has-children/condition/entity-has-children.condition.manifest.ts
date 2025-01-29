import { UMB_ENTITY_HAS_CHILDREN_CONDITION_ALIAS } from './constants.js';
import type { ManifestCondition } from '@umbraco-cms/backoffice/extension-api';

export const manifest: ManifestCondition = {
	type: 'condition',
	name: 'Entity Has Children Condition',
	alias: UMB_ENTITY_HAS_CHILDREN_CONDITION_ALIAS,
	api: () => import('./entity-has-children.condition.js'),
};
