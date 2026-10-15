export const UMB_MULTIPLE_MEMBER_PICKER_PROPERTY_EDITOR_VALUE_TYPE = 'Umbraco.MemberPicker.Multiple' as const;

declare global {
	interface UmbValueTypeMap {
		[UMB_MULTIPLE_MEMBER_PICKER_PROPERTY_EDITOR_VALUE_TYPE]: Array<string>;
	}
}
