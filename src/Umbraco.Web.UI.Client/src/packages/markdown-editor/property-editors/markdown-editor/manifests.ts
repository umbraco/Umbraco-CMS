import { manifest as schemaManifest } from './Umbraco.MarkdownEditor.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'propertyValuePreset',
		forPropertyEditorSchemaAlias: 'Umbraco.MarkdownEditor',
		alias: 'Umbraco.MarkdownEditor',
		name: 'Markdown Editor Property Value Preset',
		api: () => import('./property-value-preset.MarkdownEditor.js'),
	},
	{
		type: 'propertyEditorUi',
		alias: 'Umb.PropertyEditorUi.MarkdownEditor',
		name: 'Markdown Editor Property Editor UI',
		element: () => import('./property-editor-ui-markdown-editor.element.js'),
		meta: {
			label: 'Markdown Editor',
			propertyEditorSchemaAlias: 'Umbraco.MarkdownEditor',
			icon: 'icon-code',
			group: 'richContent',
			supportsReadOnly: true,
			settings: {
				properties: [
					{
						alias: 'preview',
						label: 'Preview',
						description: 'Display a live preview',
						propertyEditorUiAlias: 'Umb.PropertyEditorUi.Toggle',
					},
					{
						alias: 'defaultValue',
						label: 'Default value',
						description: 'If value is blank, the editor will show this',
						propertyEditorUiAlias: 'Umb.PropertyEditorUi.MarkdownEditor',
					},
					{
						alias: 'overlaySize',
						label: 'Overlay Size',
						description: 'Select the width of the overlay.',
						propertyEditorUiAlias: 'Umb.PropertyEditorUi.OverlaySize',
					},
				],
			},
		},
	},
	schemaManifest,
];
