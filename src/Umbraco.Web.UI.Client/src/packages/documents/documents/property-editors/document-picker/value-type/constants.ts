export const UMB_DOCUMENT_PICKER_PROPERTY_EDITOR_VALUE_TYPE = 'Umbraco.ContentPicker' as const;

declare global {
	interface UmbValueTypeMap {
		[UMB_DOCUMENT_PICKER_PROPERTY_EDITOR_VALUE_TYPE]: string;
	}
}
