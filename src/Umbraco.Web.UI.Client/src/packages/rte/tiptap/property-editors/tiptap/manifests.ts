import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<ManifestTypes> = [
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
						alias: 'extensions',
						label: 'Extensions',
						description: 'Extensions to enable',
						propertyEditorUiAlias: 'Umb.PropertyEditorUi.Tiptap.ExtensionsConfiguration',
						weight: 5,
					},
					{
						alias: 'toolbar',
						label: 'Toolbar',
						description: 'Pick the toolbar options that should be available when editing',
						propertyEditorUiAlias: 'Umb.PropertyEditorUi.Tiptap.ToolbarConfiguration',
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
