import type { UMB_ENTITY_HAS_CHILDREN_CONDITION_ALIAS } from './constants.js';
import type { UmbConditionConfigBase } from '@umbraco-cms/backoffice/extension-api';

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface UmbEntityHasChildrenConditionConfig
	extends UmbConditionConfigBase<typeof UMB_ENTITY_HAS_CHILDREN_CONDITION_ALIAS> {}

declare global {
	interface UmbExtensionConditionConfigMap {
		UmbEntityHasChildrenConditionConfig: UmbEntityHasChildrenConditionConfig;
	}
}
