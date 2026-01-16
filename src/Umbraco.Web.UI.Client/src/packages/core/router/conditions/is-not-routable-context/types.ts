import type { UMB_IS_NOT_ROUTABLE_CONTEXT_CONDITION_ALIAS } from './constants.js';
import type { UmbConditionConfigBase } from '@umbraco-cms/backoffice/extension-api';

export type UmbIsNotRoutableContextConditionConfig = UmbConditionConfigBase<
	typeof UMB_IS_NOT_ROUTABLE_CONTEXT_CONDITION_ALIAS
>;

declare global {
	interface UmbExtensionConditionConfigMap {
		UmbIsNotRoutableContextConditionConfig: UmbIsNotRoutableContextConditionConfig;
	}
}
