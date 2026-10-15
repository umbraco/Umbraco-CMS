export const UMB_MULTIPLE_DOCUMENT_PICKER_PROPERTY_EDITOR_VALUE_TYPE = 'Umbraco.DocumentPicker.Multiple' as const;

declare global {
	interface UmbValueTypeMap {
		[UMB_MULTIPLE_DOCUMENT_PICKER_PROPERTY_EDITOR_VALUE_TYPE]: Array<string>;
	}
}
