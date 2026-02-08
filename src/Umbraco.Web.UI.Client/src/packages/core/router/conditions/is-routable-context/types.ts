import type { UMB_IS_ROUTABLE_CONTEXT_CONDITION_ALIAS } from './constants.js';
import type { UmbConditionConfigBase } from '@umbraco-cms/backoffice/extension-api';

export type UmbIsRoutableContextConditionConfig = UmbConditionConfigBase<
	typeof UMB_IS_ROUTABLE_CONTEXT_CONDITION_ALIAS
>;

declare global {
	interface UmbExtensionConditionConfigMap {
		UmbIsRoutableContextConditionConfig: UmbIsRoutableContextConditionConfig;
	}
}
