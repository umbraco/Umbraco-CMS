import type { ManifestPropertyEditorUi } from '@umbraco-cms/backoffice/property-editor';

export const manifests: Array<ManifestPropertyEditorUi> = [
	{
		type: 'propertyEditorUi',
		alias: 'Umb.PropertyEditorUi.Tiptap',
		name: 'Rich Text Editor [Tiptap] Property Editor UI',
		element: () => import('./property-editor-ui-tiptap.element.js'),
		meta: {
			label: 'Rich Text Editor [Tiptap]',
			propertyEditorSchemaAlias: 'Umbraco.RichText',
			icon: 'icon-browser-window',
			group: 'richContent',
			settings: {
				properties: [
					{
						alias: 'toolbar',
						label: 'Toolbar',
						description: 'Pick the toolbar items that should be available when editing',
						propertyEditorUiAlias: 'Umb.PropertyEditorUi.Tiptap.ToolbarConfiguration',
						weight: 5,
					},
					{
						alias: 'extensions',
						label: 'Capabilities',
						description: 'Enable extensions enhance the capabilities of the Tiptap editor.',
						propertyEditorUiAlias: 'Umb.PropertyEditorUi.Tiptap.ExtensionsConfiguration',
						weight: 10,
					},
					{
						alias: 'dimensions',
						label: 'Dimensions',
						description: 'Set the maximum width and height of the editor',
						propertyEditorUiAlias: 'Umb.PropertyEditorUI.TinyMCE.DimensionsConfiguration',
						weight: 20,
					},
					{
						alias: 'maxImageSize',
						label: 'Maximum size for inserted images',
						description: 'Maximum width or height - enter 0 to disable resizing',
						propertyEditorUiAlias: 'Umb.PropertyEditorUI.TinyMCE.MaxImageSizeConfiguration',
						weight: 40,
					},
					{
						alias: 'overlaySize',
						label: 'Overlay Size',
						description: 'Select the width of the overlay (link picker)',
						propertyEditorUiAlias: 'Umb.PropertyEditorUi.OverlaySize',
						weight: 50,
					},
				],
				defaultData: [
					{
						alias: 'toolbar',
						value: [
							[
								['Umb.Tiptap.Toolbar.SourceEditor'],
								['Umb.Tiptap.Toolbar.Bold', 'Umb.Tiptap.Toolbar.Italic', 'Umb.Tiptap.Toolbar.Underline'],
								[
									'Umb.Tiptap.Toolbar.TextAlignLeft',
									'Umb.Tiptap.Toolbar.TextAlignCenter',
									'Umb.Tiptap.Toolbar.TextAlignRight',
								],
								['Umb.Tiptap.Toolbar.BulletList', 'Umb.Tiptap.Toolbar.OrderedList'],
								['Umb.Tiptap.Toolbar.Blockquote', 'Umb.Tiptap.Toolbar.HorizontalRule'],
								['Umb.Tiptap.Toolbar.Link', 'Umb.Tiptap.Toolbar.Unlink'],
								['Umb.Tiptap.Toolbar.MediaPicker', 'Umb.Tiptap.Toolbar.EmbeddedMedia'],
							],
						],
					},
					{ alias: 'maxImageSize', value: 500 },
					{ alias: 'overlaySize', value: 'medium' },
				],
			},
		},
	},
	{
		type: 'propertyEditorUi',
		alias: 'Umb.PropertyEditorUi.Tiptap.ToolbarConfiguration',
		name: 'Tiptap Toolbar Property Editor UI',
		js: () => import('./components/property-editor-ui-tiptap-toolbar-configuration.element.js'),
		meta: {
			label: 'Tiptap Toolbar Configuration',
			icon: 'icon-autofill',
			group: 'common',
		},
	},
	{
		type: 'propertyEditorUi',
		alias: 'Umb.PropertyEditorUi.Tiptap.ExtensionsConfiguration',
		name: 'Tiptap Extensions Property Editor UI',
		js: () => import('./components/property-editor-ui-tiptap-extensions-configuration.element.js'),
		meta: {
			label: 'Tiptap Extensions Configuration',
			icon: 'icon-autofill',
			group: 'common',
		},
	},
];
