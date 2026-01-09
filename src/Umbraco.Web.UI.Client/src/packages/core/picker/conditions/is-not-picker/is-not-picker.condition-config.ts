import type { UMB_IS_NOT_PICKER_CONDITION_ALIAS } from './constants.js';
import type { UmbConditionConfigBase } from '@umbraco-cms/backoffice/extension-api';

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface UmbIsNotPickerConditionConfig
	extends UmbConditionConfigBase<typeof UMB_IS_NOT_PICKER_CONDITION_ALIAS> {}

declare global {
	interface UmbExtensionConditionConfigMap {
		UmbIsNotPickerConditionConfig: UmbIsNotPickerConditionConfig;
	}
}
