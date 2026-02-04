import type { UMB_IS_PRODUCTION_MODE_CONDITION_ALIAS } from './constants.js';
import type { UmbConditionConfigBase } from '@umbraco-cms/backoffice/extension-api';

export interface UmbIsProductionModeConditionConfig
	extends UmbConditionConfigBase<typeof UMB_IS_PRODUCTION_MODE_CONDITION_ALIAS> {
	/**
	 * If true (default), the condition is permitted when in Production mode.
	 * If false, the condition is permitted when NOT in Production mode.
	 */
	match?: boolean;
}

declare global {
	interface UmbExtensionConditionConfigMap {
		umbIsProductionModeConditionConfig: UmbIsProductionModeConditionConfig;
	}
}
