export const UMB_CHECKBOX_LIST_PROPERTY_EDITOR_VALUE_TYPE = 'Umbraco.CheckBoxList' as const;

declare global {
	interface UmbValueTypeMap {
		[UMB_CHECKBOX_LIST_PROPERTY_EDITOR_VALUE_TYPE]: Array<string>;
	}
}
