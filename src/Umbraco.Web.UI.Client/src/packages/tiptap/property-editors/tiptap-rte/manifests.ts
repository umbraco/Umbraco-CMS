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
						alias: 'extensions',
						label: '#tiptap_config_extensions',
						description: `Choose which Tiptap extensions to enable.

_Once enabled, the related actions will be available for the toolbar and statusbar._`,
						propertyEditorUiAlias: 'Umb.PropertyEditorUi.Tiptap.ExtensionsConfiguration',
						weight: 10,
					},
					{
						alias: 'toolbar',
						label: '#tiptap_config_toolbar',
						description: `Design the available actions.

_Drag and drop the available actions onto the toolbar._`,
						propertyEditorUiAlias: 'Umb.PropertyEditorUi.Tiptap.ToolbarConfiguration',
						weight: 15,
					},
					{
						alias: 'statusbar',
						label: '#tiptap_config_statusbar',
						description: `Design the available statuses.

_Drag and drop the available actions onto the statusbar areas._`,
						propertyEditorUiAlias: 'Umb.PropertyEditorUi.Tiptap.StatusbarConfiguration',
						weight: 18,
					},
					{
						alias: 'stylesheets',
						label: '#treeHeaders_stylesheets',
						description: 'Pick the stylesheets whose editor styles should be available when editing.',
						propertyEditorUiAlias: 'Umb.PropertyEditorUi.StylesheetPicker',
						weight: 20,
					},
					{
						alias: 'dimensions',
						label: '#general_dimensions',
						description: '{#tiptap_config_dimensions_description}',
						propertyEditorUiAlias: 'Umb.PropertyEditorUI.Dimensions',
						weight: 30,
					},
					{
						alias: 'maxImageSize',
						label: '#rte_config_maxImageSize',
						description: '{#rte_config_maxImageSize_description}',
						propertyEditorUiAlias: 'Umb.PropertyEditorUi.Integer',
						config: [{ alias: 'min', value: 0 }],
						weight: 40,
					},
					{
						alias: 'overlaySize',
						label: '#rte_config_overlaySize',
						description: '{#rte_config_overlaySize_description}',
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
];
