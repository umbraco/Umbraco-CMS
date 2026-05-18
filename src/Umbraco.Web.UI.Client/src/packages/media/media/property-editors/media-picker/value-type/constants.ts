import type { UmbMediaPickerValueModel } from '../../types.js';

export const UMB_MEDIA_PICKER_PROPERTY_EDITOR_VALUE_TYPE = 'Umbraco.MediaPicker3' as const;

declare global {
	interface UmbValueTypeMap {
		[UMB_MEDIA_PICKER_PROPERTY_EDITOR_VALUE_TYPE]: UmbMediaPickerValueModel | undefined;
	}
}
