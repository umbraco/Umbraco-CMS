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
						alias: 'toolbar',
						label: 'Toolbar - NOT IMPLEMENTED',
						description: 'Pick the toolbar options that should be available when editing',
						propertyEditorUiAlias: 'Umb.PropertyEditorUi.Tiptap.ToolbarConfiguration',
						weight: 10,
						config: [
							{
								alias: 'toolbar',
								value: [
									// Clipboard Group
									{ alias: 'undo', label: 'Undo', icon: 'undo', category: 'clipboard' },
									{ alias: 'redo', label: 'Redo', icon: 'redo', category: 'clipboard' },
									{ alias: 'cut', label: 'Cut', icon: 'cut', category: 'clipboard' },
									{ alias: 'copy', label: 'Copy', icon: 'copy', category: 'clipboard' },
									{ alias: 'paste', label: 'Paste', icon: 'paste', category: 'clipboard' },

									// Formatting Group
									{ alias: 'bold', label: 'Bold', icon: 'bold', category: 'formatting' },
									{ alias: 'italic', label: 'Italic', icon: 'italic', category: 'formatting' },
									{ alias: 'underline', label: 'Underline', icon: 'underline', category: 'formatting' },
									{ alias: 'strikethrough', label: 'Strikethrough', icon: 'strike-through', category: 'formatting' },
									{ alias: 'removeformat', label: 'Remove format', icon: 'remove-formatting', category: 'formatting' },

									// Color Group
									{ alias: 'forecolor', label: 'Text color', icon: 'text-color', category: 'color' },
									{ alias: 'backcolor', label: 'Background color', icon: 'highlight-bg-color', category: 'color' },

									// Alignment Group
									{ alias: 'alignleft', label: 'Align left', icon: 'align-left', category: 'alignment' },
									{ alias: 'aligncenter', label: 'Align center', icon: 'align-center', category: 'alignment' },
									{ alias: 'alignright', label: 'Align right', icon: 'align-right', category: 'alignment' },
									{ alias: 'alignjustify', label: 'Justify justify', icon: 'align-justify', category: 'alignment' },

									// List Group
									{ alias: 'bullist', label: 'Bullet list', icon: 'unordered-list', category: 'list' },
									{ alias: 'numlist', label: 'Numbered list', icon: 'ordered-list', category: 'list' },

									// Indentation Group
									{ alias: 'outdent', label: 'Outdent', icon: 'outdent', category: 'indentation' },
									{ alias: 'indent', label: 'Indent', icon: 'indent', category: 'indentation' },

									// Insert Elements Group
									{ alias: 'anchor', label: 'Anchor', icon: 'bookmark', category: 'insert' },
									{ alias: 'table', label: 'Table', icon: 'table', category: 'insert' },
									{ alias: 'hr', label: 'Horizontal rule', icon: 'horizontal-rule', category: 'insert' },
									{ alias: 'charmap', label: 'Character map', icon: 'insert-character', category: 'insert' },

									// Direction Group
									{ alias: 'rtl', label: 'Right to left', icon: 'rtl', category: 'direction' },
									{ alias: 'ltr', label: 'Left to right', icon: 'ltr', category: 'direction' },

									// Text Transformation Group
									{ alias: 'subscript', label: 'Subscript', icon: 'subscript', category: 'text-transformation' },
									{ alias: 'superscript', label: 'Superscript', icon: 'superscript', category: 'text-transformation' },

									// Styling and Font Group
									{ alias: 'styles', label: 'Style select', icon: 'permanent-pen', category: 'styling' },
									{ alias: 'fontname', label: 'Font select', icon: 'text-color', category: 'styling' },
									{ alias: 'fontsize', label: 'Font size', icon: 'text-color', category: 'styling' },

									// Block Element Group
									{ alias: 'blockquote', label: 'Blockquote', icon: 'quote', category: 'block-elements' },
									{ alias: 'formatblock', label: 'Format block', icon: 'format', category: 'block-elements' },
								],
							},
						],
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
						alias: 'mode',
						label: 'Mode - NOT IMPLEMENTED',
						description: 'Select the mode for the editor',
						propertyEditorUiAlias: 'Umb.PropertyEditorUi.RadioButtonList',
						config: [{ alias: 'items', value: ['Classic', 'Inline'] }],
						weight: 50,
					},
				],
				defaultData: [
					// {
					// 	alias: 'toolbar',
					// 	value: [
					// 		'styles',
					// 		'bold',
					// 		'italic',
					// 		'alignleft',
					// 		'aligncenter',
					// 		'alignright',
					// 		'bullist',
					// 		'numlist',
					// 		'outdent',
					// 		'indent',
					// 		'sourcecode',
					// 		'link',
					// 	],
					// },
					// { alias: 'mode', value: 'Classic' },
					// { alias: 'maxWidth', value: 800 },
					// { alias: 'maxHeight', value: 500 },
				],
			},
		},
	},
];
