import type { UmbLinkPickerLink } from '../../link-picker-modal/index.js';

export const UMB_MULTI_URL_PICKER_PROPERTY_EDITOR_VALUE_TYPE = 'Umbraco.MultiUrlPicker' as const;

export const UMB_SINGLE_URL_PICKER_PROPERTY_EDITOR_VALUE_TYPE = 'Umbraco.UrlPicker.Single' as const;

declare global {
	interface UmbValueTypeMap {
		[UMB_MULTI_URL_PICKER_PROPERTY_EDITOR_VALUE_TYPE]: Array<UmbLinkPickerLink>;
		[UMB_SINGLE_URL_PICKER_PROPERTY_EDITOR_VALUE_TYPE]: Array<UmbLinkPickerLink>;
	}
}
