export const UMB_MARKDOWN_EDITOR_PROPERTY_EDITOR_VALUE_TYPE = 'Umbraco.MarkdownEditor' as const;

declare global {
	interface UmbValueTypeMap {
		[UMB_MARKDOWN_EDITOR_PROPERTY_EDITOR_VALUE_TYPE]: string;
	}
}
