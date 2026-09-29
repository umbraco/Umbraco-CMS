export const UMB_USER_PICKER_PROPERTY_EDITOR_VALUE_TYPE = 'Umbraco.UserPicker' as const;

declare global {
	interface UmbValueTypeMap {
		[UMB_USER_PICKER_PROPERTY_EDITOR_VALUE_TYPE]: string;
	}
}
