export const UMB_DATE_TIME_UNSPECIFIED_PROPERTY_EDITOR_VALUE_TYPE = 'Umbraco.DateTimeUnspecified' as const;

declare global {
	interface UmbValueTypeMap {
		[UMB_DATE_TIME_UNSPECIFIED_PROPERTY_EDITOR_VALUE_TYPE]: { date: string | null; timeZone: null };
	}
}
