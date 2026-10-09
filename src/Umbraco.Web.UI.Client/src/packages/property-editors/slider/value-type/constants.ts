import type { UmbSliderPropertyEditorUiValue } from '../types.js';

export const UMB_SLIDER_PROPERTY_EDITOR_VALUE_TYPE = 'Umbraco.Slider' as const;

export const UMB_RANGE_SLIDER_PROPERTY_EDITOR_VALUE_TYPE = 'Umbraco.Slider.Range' as const;

declare global {
	interface UmbValueTypeMap {
		[UMB_SLIDER_PROPERTY_EDITOR_VALUE_TYPE]: UmbSliderPropertyEditorUiValue;
		[UMB_RANGE_SLIDER_PROPERTY_EDITOR_VALUE_TYPE]: UmbSliderPropertyEditorUiValue;
	}
}
