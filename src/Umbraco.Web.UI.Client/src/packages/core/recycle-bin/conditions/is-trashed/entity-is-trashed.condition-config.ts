import type { UMB_ENTITY_IS_TRASHED_CONDITION_ALIAS } from './constants.js';
import type { UmbConditionConfigBase } from '@umbraco-cms/backoffice/extension-api';

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface UmbEntityIsTrashedConditionConfig
	extends UmbConditionConfigBase<typeof UMB_ENTITY_IS_TRASHED_CONDITION_ALIAS> {}

declare global {
	interface UmbExtensionConditionConfigMap {
		UmbEntityIsTrashedConditionConfig: UmbEntityIsTrashedConditionConfig;
	}
}
