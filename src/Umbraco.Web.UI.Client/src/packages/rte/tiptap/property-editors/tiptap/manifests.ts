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
									{ alias: 'undo', label: 'Undo', icon: 'undo', group: 'clipboard' },
									{ alias: 'redo', label: 'Redo', icon: 'redo', group: 'clipboard' },
									{ alias: 'cut', label: 'Cut', icon: 'cut', group: 'clipboard' },
									{ alias: 'copy', label: 'Copy', icon: 'copy', group: 'clipboard' },
									{ alias: 'paste', label: 'Paste', icon: 'paste', group: 'clipboard' },

									// Formatting Group
									{ alias: 'bold', label: 'Bold', icon: 'bold', group: 'formatting' },
									{ alias: 'italic', label: 'Italic', icon: 'italic', group: 'formatting' },
									{ alias: 'underline', label: 'Underline', icon: 'underline', group: 'formatting' },
									{ alias: 'strikethrough', label: 'Strikethrough', icon: 'strike-through', group: 'formatting' },
									{ alias: 'removeformat', label: 'Remove format', icon: 'remove-formatting', group: 'formatting' },

									// Color Group
									{ alias: 'forecolor', label: 'Text color', icon: 'text-color', group: 'color' },
									{ alias: 'backcolor', label: 'Background color', icon: 'highlight-bg-color', group: 'color' },

									// Alignment Group
									{ alias: 'alignleft', label: 'Align left', icon: 'align-left', group: 'alignment' },
									{ alias: 'aligncenter', label: 'Align center', icon: 'align-center', group: 'alignment' },
									{ alias: 'alignright', label: 'Align right', icon: 'align-right', group: 'alignment' },
									{ alias: 'alignjustify', label: 'Justify justify', icon: 'align-justify', group: 'alignment' },

									// List Group
									{ alias: 'bullist', label: 'Bullet list', icon: 'unordered-list', group: 'list' },
									{ alias: 'numlist', label: 'Numbered list', icon: 'ordered-list', group: 'list' },

									// Indentation Group
									{ alias: 'outdent', label: 'Outdent', icon: 'outdent', group: 'indentation' },
									{ alias: 'indent', label: 'Indent', icon: 'indent', group: 'indentation' },

									// Insert Elements Group
									{ alias: 'anchor', label: 'Anchor', icon: 'bookmark', group: 'insert' },
									{ alias: 'table', label: 'Table', icon: 'table', group: 'insert' },
									{ alias: 'hr', label: 'Horizontal rule', icon: 'horizontal-rule', group: 'insert' },
									{ alias: 'charmap', label: 'Character map', icon: 'insert-character', group: 'insert' },

									// Direction Group
									{ alias: 'rtl', label: 'Right to left', icon: 'rtl', group: 'direction' },
									{ alias: 'ltr', label: 'Left to right', icon: 'ltr', group: 'direction' },

									// Text Transformation Group
									{ alias: 'subscript', label: 'Subscript', icon: 'subscript', group: 'text-transformation' },
									{ alias: 'superscript', label: 'Superscript', icon: 'superscript', group: 'text-transformation' },

									// Styling and Font Group
									{ alias: 'styles', label: 'Style select', icon: 'permanent-pen', group: 'styling' },
									{ alias: 'fontname', label: 'Font select', icon: 'text-color', group: 'styling' },
									{ alias: 'fontsize', label: 'Font size', icon: 'text-color', group: 'styling' },

									// Block Element Group
									{ alias: 'blockquote', label: 'Blockquote', icon: 'quote', group: 'block-elements' },
									{ alias: 'formatblock', label: 'Format block', icon: 'format', group: 'block-elements' },
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
