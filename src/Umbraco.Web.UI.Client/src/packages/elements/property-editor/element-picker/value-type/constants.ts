export const UMB_ELEMENT_PICKER_PROPERTY_EDITOR_VALUE_TYPE = 'Umbraco.ElementPicker' as const;

declare global {
	interface UmbValueTypeMap {
		[UMB_ELEMENT_PICKER_PROPERTY_EDITOR_VALUE_TYPE]: Array<string>;
	}
}
