import type { UMB_PROPERTY_HAS_VALUE_CONDITION_ALIAS } from './constants.js';
import type { UmbConditionConfigBase } from '@umbraco-cms/backoffice/extension-api';

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface UmbPropertyHasValueConditionConfig
	extends UmbConditionConfigBase<typeof UMB_PROPERTY_HAS_VALUE_CONDITION_ALIAS> {}

declare global {
	interface UmbExtensionConditionConfigMap {
		UmbPropertyHasValueConditionConfig: UmbPropertyHasValueConditionConfig;
	}
}
