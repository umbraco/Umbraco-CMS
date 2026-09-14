import type { UmbContentPickerSourceType } from '../types.js';
import type { UmbConditionConfigBase } from '@umbraco-cms/backoffice/extension-api';
import type { UMB_CONTENT_PICKER_SOURCE_TYPE_CONDITION_ALIAS } from './constants.js';

export interface UmbContentPickerSourceTypeConditionConfig
	extends UmbConditionConfigBase<typeof UMB_CONTENT_PICKER_SOURCE_TYPE_CONDITION_ALIAS> {
	/**
	 * The source type the Content Picker must be configured for.
	 */
	match: UmbContentPickerSourceType;
}

declare global {
	interface UmbExtensionConditionConfigMap {
		umbContentPickerSourceType: UmbContentPickerSourceTypeConditionConfig;
	}
}
