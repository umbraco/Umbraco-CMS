export const UMB_DATE_TIME_VALUE_TYPE = 'Umb.ValueType.DateTime' as const;

declare global {
	interface UmbValueTypeMap {
		[UMB_DATE_TIME_VALUE_TYPE]: string;
	}
}
