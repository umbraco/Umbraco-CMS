export const UMB_CODE_EDITOR_PROPERTY_EDITOR_VALUE_TYPE = 'Umbraco.Plain.String' as const;

declare global {
	interface UmbValueTypeMap {
		[UMB_CODE_EDITOR_PROPERTY_EDITOR_VALUE_TYPE]: string;
	}
}
