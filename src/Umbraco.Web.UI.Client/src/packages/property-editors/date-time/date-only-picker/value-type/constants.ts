export const UMB_DATE_ONLY_PROPERTY_EDITOR_VALUE_TYPE = 'Umbraco.DateOnly' as const;

declare global {
	interface UmbValueTypeMap {
		[UMB_DATE_ONLY_PROPERTY_EDITOR_VALUE_TYPE]: { date: string | null; timeZone: null };
	}
}
