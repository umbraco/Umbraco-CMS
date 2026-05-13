export const UMB_MEMBER_PICKER_PROPERTY_EDITOR_VALUE_TYPE = 'Umbraco.MemberPicker' as const;

declare global {
	interface UmbValueTypeMap {
		[UMB_MEMBER_PICKER_PROPERTY_EDITOR_VALUE_TYPE]: string | undefined;
	}
}
