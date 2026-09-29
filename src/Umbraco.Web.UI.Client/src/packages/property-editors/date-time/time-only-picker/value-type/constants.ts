export const UMB_TIME_ONLY_PROPERTY_EDITOR_VALUE_TYPE = 'Umbraco.TimeOnly' as const;

declare global {
	interface UmbValueTypeMap {
		[UMB_TIME_ONLY_PROPERTY_EDITOR_VALUE_TYPE]: { date: string | null; timeZone: null };
	}
}
