export const UMB_MEMBER_GROUP_UNIQUES_VALUE_TYPE = 'Umb.ValueType.MemberGroup.Uniques' as const;

declare global {
	interface UmbValueTypeMap {
		[UMB_MEMBER_GROUP_UNIQUES_VALUE_TYPE]: Array<string>;
	}
}
