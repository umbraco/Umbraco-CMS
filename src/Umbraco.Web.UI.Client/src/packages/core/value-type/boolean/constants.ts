export const UMB_BOOLEAN_VALUE_TYPE = 'Umb.ValueType.Boolean' as const;

declare global {
	interface UmbValueTypeMap {
		[UMB_BOOLEAN_VALUE_TYPE]: boolean;
	}
}
