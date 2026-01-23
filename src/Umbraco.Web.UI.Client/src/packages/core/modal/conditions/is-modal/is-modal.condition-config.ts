import type { UMB_IS_MODAL_CONDITION_ALIAS } from './constants.js';
import type { UmbConditionConfigBase } from '@umbraco-cms/backoffice/extension-api';

export interface UmbIsModalConditionConfig extends UmbConditionConfigBase<typeof UMB_IS_MODAL_CONDITION_ALIAS> {
	/**
	 * The expected modal state to match.
	 * - `true`: Condition is permitted when inside a modal (default)
	 * - `false`: Condition is permitted when NOT inside a modal
	 */
	match?: boolean;
}

declare global {
	interface UmbExtensionConditionConfigMap {
		UmbIsModalConditionConfig: UmbIsModalConditionConfig;
	}
}
