import type { UMB_IS_ROUTABLE_CONTEXT_CONDITION_ALIAS } from './constants.js';
import type { UmbConditionConfigBase } from '@umbraco-cms/backoffice/extension-api';

export interface UmbIsRoutableContextConditionConfig
	extends UmbConditionConfigBase<typeof UMB_IS_ROUTABLE_CONTEXT_CONDITION_ALIAS> {
	/**
	 * The expected routable context state to match.
	 * - `true`: Condition is permitted when inside a routable context (default)
	 * - `false`: Condition is permitted when NOT inside a routable context
	 */
	match?: boolean;
}

declare global {
	interface UmbExtensionConditionConfigMap {
		UmbIsRoutableContextConditionConfig: UmbIsRoutableContextConditionConfig;
	}
}
