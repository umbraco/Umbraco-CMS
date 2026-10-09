export const UMB_STRING_VALUE_TYPE = 'Umb.ValueType.String' as const;

declare global {
	interface UmbValueTypeMap {
		[UMB_STRING_VALUE_TYPE]: string;
	}
}
