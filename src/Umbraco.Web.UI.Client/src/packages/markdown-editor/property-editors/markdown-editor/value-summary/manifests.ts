const UMB_MARKDOWN_EDITOR_PROPERTY_EDITOR_VALUE_TYPE = 'Umbraco.MarkdownEditor' as const;

declare global {
	interface UmbValueTypeMap {
		[UMB_MARKDOWN_EDITOR_PROPERTY_EDITOR_VALUE_TYPE]: string;
	}
}

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'valueSummary',
		kind: 'default',
		alias: 'Umb.ValueSummary.PropertyEditor.MarkdownEditor',
		name: 'Markdown Editor Property Editor Value Summary',
		forValueType: UMB_MARKDOWN_EDITOR_PROPERTY_EDITOR_VALUE_TYPE,
		element: () => import('./value-summary.element.js'),
	},
];
