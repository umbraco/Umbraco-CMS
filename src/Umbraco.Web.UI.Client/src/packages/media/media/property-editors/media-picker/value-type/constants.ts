import type { UmbMediaPickerValueModel } from '../../types.js';

export const UMB_MEDIA_PICKER_PROPERTY_EDITOR_VALUE_TYPE = 'Umbraco.MediaPicker3' as const;

export const UMB_SINGLE_MEDIA_PICKER_PROPERTY_EDITOR_VALUE_TYPE = 'Umbraco.SingleMediaPicker' as const;

declare global {
	interface UmbValueTypeMap {
		[UMB_MEDIA_PICKER_PROPERTY_EDITOR_VALUE_TYPE]: UmbMediaPickerValueModel;
		[UMB_SINGLE_MEDIA_PICKER_PROPERTY_EDITOR_VALUE_TYPE]: UmbMediaPickerValueModel;
	}
}
