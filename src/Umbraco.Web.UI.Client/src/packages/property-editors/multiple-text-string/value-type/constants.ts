export const UMB_MULTIPLE_TEXT_STRING_PROPERTY_EDITOR_VALUE_TYPE = 'Umbraco.MultipleTextstring' as const;

declare global {
	interface UmbValueTypeMap {
		[UMB_MULTIPLE_TEXT_STRING_PROPERTY_EDITOR_VALUE_TYPE]: Array<string> | undefined;
	}
}
