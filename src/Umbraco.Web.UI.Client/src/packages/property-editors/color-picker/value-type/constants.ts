import type { UmbColorPickerPropertyEditorValue } from '../types.js';

export const UMB_COLOR_PICKER_PROPERTY_EDITOR_VALUE_TYPE = 'Umbraco.ColorPicker' as const;

declare global {
	interface UmbValueTypeMap {
		[UMB_COLOR_PICKER_PROPERTY_EDITOR_VALUE_TYPE]: UmbColorPickerPropertyEditorValue;
	}
}
