export const UMB_TAGS_PROPERTY_EDITOR_VALUE_TYPE = 'Umbraco.Tags' as const;

declare global {
	interface UmbValueTypeMap {
		[UMB_TAGS_PROPERTY_EDITOR_VALUE_TYPE]: Array<string> | undefined;
	}
}
