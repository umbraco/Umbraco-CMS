import type { UmbDateTimeWithTimeZonePropertyEditorValue } from './types.js';

export const UMB_DATE_TIME_WITH_TIME_ZONE_PROPERTY_EDITOR_VALUE_TYPE = 'Umbraco.DateTimeWithTimeZone' as const;

declare global {
	interface UmbValueTypeMap {
		[UMB_DATE_TIME_WITH_TIME_ZONE_PROPERTY_EDITOR_VALUE_TYPE]: UmbDateTimeWithTimeZonePropertyEditorValue;
	}
}
