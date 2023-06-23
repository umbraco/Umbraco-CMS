import { manifests as configuration } from './config/manifests.js';
import type { ManifestPropertyEditorUi } from '@umbraco-cms/backoffice/extension-registry';

const manifest: ManifestPropertyEditorUi = {
	type: 'propertyEditorUi',
	alias: 'Umb.PropertyEditorUi.TinyMCE',
	name: 'Rich Text Editor Property Editor UI',
	loader: () => import('./property-editor-ui-tiny-mce.element.js'),
	meta: {
		label: 'Rich Text Editor',
		propertyEditorSchemaAlias: 'Umbraco.TinyMCE',
		icon: 'umb:browser-window',
		group: 'richText',
		settings: {
			properties: [
				{
					alias: 'toolbar',
					label: 'Toolbar',
					description: 'Pick the toolbar options that should be available when editing',
					propertyEditorUiAlias: 'Umb.PropertyEditorUI.TinyMCE.ToolbarConfiguration',
					config: [
						{
							alias: 'toolbar',
							value: [
								{
									alias: 'removeformat',
									label: 'Remove format',
									icon: 'remove-formatting',
								},
								{
									alias: 'undo',
									label: 'Undo',
									icon: 'undo',
								},
								{
									alias: 'redo',
									label: 'Redo',
									icon: 'redo',
								},
								{
									alias: 'cut',
									label: 'Cut',
									icon: 'cut',
								},
								{
									alias: 'copy',
									label: 'Copy',
									icon: 'copy',
								},
								{
									alias: 'paste',
									label: 'Paste',
									icon: 'paste',
								},
								{
									alias: 'styles',
									label: 'Style select',
									icon: 'permanent-pen',
								},
								{
									alias: 'forecolor',
									label: 'Text color',
									icon: 'text-color',
								},
								{
									alias: 'backcolor',
									label: 'Background color',
									icon: 'background-color',
								},
								{
									alias: 'blockquote',
									label: 'Blockquote',
									icon: 'quote',
								},
								{
									alias: 'bold',
									label: 'Bold',
									icon: 'bold',
								},
								{
									alias: 'italic',
									label: 'Italic',
									icon: 'italic',
								},
								{
									alias: 'underline',
									label: 'Underline',
									icon: 'underline',
								},
								{
									alias: 'strikethrough',
									label: 'Strikethrough',
									icon: 'strike-through',
								},
								{
									alias: 'alignleft',
									label: 'Align left',
									icon: 'align-left',
								},
								{
									alias: 'aligncenter',
									label: 'Align center',
									icon: 'align-center',
								},
								{
									alias: 'alignright',
									label: 'Align right',
									icon: 'align-right',
								},
								{
									alias: 'alignjustify',
									label: 'Justify justify',
									icon: 'align-justify',
								},
								{
									alias: 'bullist',
									label: 'Bullet list',
									icon: 'unordered-list',
								},
								{
									alias: 'numlist',
									label: 'Numbered list',
									icon: 'ordered-list',
								},
								{
									alias: 'outdent',
									label: 'Outdent',
									icon: 'outdent',
								},
								{
									alias: 'indent',
									label: 'Indent',
									icon: 'indent',
								},
								{
									alias: 'anchor',
									label: 'Anchor',
									icon: 'bookmark',
								},
								{
									alias: 'table',
									label: 'Table',
									icon: 'table',
								},
								{
									alias: 'hr',
									label: 'Horizontal rule',
									icon: 'horizontal-rule',
								},
								{
									alias: 'subscript',
									label: 'Subscript',
									icon: 'subscript',
								},
								{
									alias: 'superscript',
									label: 'Superscript',
									icon: 'superscript',
								},
								{
									alias: 'charmap',
									label: 'Character map',
									icon: 'insert-character',
								},
								{
									alias: 'rtl',
									label: 'Right to left',
									icon: 'rtl',
								},
								{
									alias: 'ltr',
									label: 'Left to right',
									icon: 'ltr',
								},
							],
						},
					],
				},
				{
					alias: 'stylesheets',
					label: 'Stylesheets',
					description: 'Pick the stylesheets whose editor styles should be available when editing',
					propertyEditorUiAlias: 'Umb.PropertyEditorUI.TinyMCE.StylesheetsConfiguration',
				},
				{
					alias: 'dimensions',
					label: 'Dimensions',
					description: 'Set the editor dimensions',
					propertyEditorUiAlias: 'Umb.PropertyEditorUI.TinyMCE.DimensionsConfiguration',
				},
				{
					alias: 'maxImageSize',
					label: 'Maximum size for inserted images',
					description: 'Maximum width or height - enter 0 to disable resizing',
					propertyEditorUiAlias: 'Umb.PropertyEditorUI.TinyMCE.MaxImageSizeConfiguration',
				},
				{
					alias: 'mode',
					label: 'Mode',
					description: 'Select the mode for the editor',
					propertyEditorUiAlias: 'Umb.PropertyEditorUi.Dropdown',
					config: [
						{
							alias: 'items',
							value: ['Classic', 'Inline'],
						},
					],
				},
				{
					alias: 'overlaySize',
					label: 'Overlay Size',
					description: 'Select the width of the overlay (link picker)',
					propertyEditorUiAlias: 'Umb.PropertyEditorUi.OverlaySize',
				},
				{
					alias: 'hideLabel',
					label: 'Hide Label',
					propertyEditorUiAlias: 'Umb.PropertyEditorUi.Toggle',
				},
			],
		},
	},
};

export const manifests = [manifest, ...configuration];
