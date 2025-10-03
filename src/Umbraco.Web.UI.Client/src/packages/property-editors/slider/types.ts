export interface UmbSliderPropertyEditorUiValueObject {
	from: number;
	to: number;
}

export type UmbSliderPropertyEditorUiValue = UmbSliderPropertyEditorUiValueObject | undefined;

/**
 * @deprecated this type will be removed in v.17.0, use `UmbPropertyEditorUISliderValue` instead
 */
export type UmbSliderValue = UmbSliderPropertyEditorUiValue;
