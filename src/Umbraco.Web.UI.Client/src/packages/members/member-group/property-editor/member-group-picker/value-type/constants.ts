export const UMB_MEMBER_GROUP_PICKER_PROPERTY_EDITOR_VALUE_TYPE = 'Umbraco.MemberGroupPicker' as const;

declare global {
	interface UmbValueTypeMap {
		[UMB_MEMBER_GROUP_PICKER_PROPERTY_EDITOR_VALUE_TYPE]: string;
	}
}
