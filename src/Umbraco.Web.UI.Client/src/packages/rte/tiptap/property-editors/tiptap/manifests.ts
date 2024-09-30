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
						label: 'Extensions',
						description: 'Extensions to enable',
						propertyEditorUiAlias: 'Umb.PropertyEditorUi.Tiptap.ExtensionsConfiguration',
						weight: 10,
					},
					{
						alias: 'maxWidth',
						label: 'MaxWidth',
						description: 'Editor max width',
						propertyEditorUiAlias: 'Umb.PropertyEditorUi.Integer',
						weight: 20,
					},
					{
						alias: 'maxHeight',
						label: 'MaxHeight',
						description: 'Editor max height',
						propertyEditorUiAlias: 'Umb.PropertyEditorUi.Integer',
						weight: 30,
					},
					{
						alias: 'maxImageSize',
						label: 'Maximum size for inserted images',
						description: 'Maximum width or height - enter 0 to disable resizing',
						propertyEditorUiAlias: 'Umb.PropertyEditorUi.Integer',
						weight: 40,
						config: [{ alias: 'min', value: 0 }],
					},
				],
				defaultData: [],
			},
		},
	},
];
