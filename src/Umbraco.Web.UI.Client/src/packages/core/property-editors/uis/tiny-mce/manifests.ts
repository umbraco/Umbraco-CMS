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
									icon: 'umb:format-clear',
								},
								{
									alias: 'undo',
									label: 'Undo',
									icon: 'umb:undo',
								},
								{
									alias: 'redo',
									label: 'Redo',
									icon: 'umb:redo',
								},
								{
									alias: 'cut',
									label: 'Cut',
									icon: 'umb:cut',
								},
								{
									alias: 'copy',
									label: 'Copy',
									icon: 'umb:copy',
								},
								{
									alias: 'paste',
									label: 'Paste',
									icon: 'umb:paste',
								},
								{
									alias: 'styles',
									label: 'Style select',
									icon: 'umb:format-color-fill',
								},
								{
									alias: 'bold',
									label: 'Bold',
									icon: 'umb:format-bold',
								},
								{
									alias: 'italic',
									label: 'Italic',
									icon: 'umb:format-italic',
								},
								{
									alias: 'underline',
									label: 'Underline',
									icon: 'umb:format-underline',
								},
								{
									alias: 'strikethrough',
									label: 'Strikethrough',
									icon: 'umb:format-strikethrough',
								},
								{
									alias: 'alignleft',
									label: 'Align left',
									icon: 'umb:align-left',
								},
								{
									alias: 'aligncenter',
									label: 'Align center',
									icon: 'umb:align-center',
								},
								{
									alias: 'alignright',
									label: 'Align right',
									icon: 'umb:align-right',
								},
								{
									alias: 'alignjustify',
									label: 'Justify full',
									icon: 'umb:align-justify',
								},
								{
									alias: 'bullist',
									label: 'Bullet list',
									icon: 'umb:list-bullet',
								},
								{
									alias: 'numlist',
									label: 'Numbered list',
									icon: 'umb:list-numbered',
								},
								{
									alias: 'outdent',
									label: 'Outdent',
									icon: 'umb:indent-left',
								},
								{
									alias: 'indent',
									label: 'Indent',
									icon: 'umb:indent-right',
								},
								{
									alias: 'anchor',
									label: 'Anchor',
									icon: 'umb:anchor',
								},
								{
									alias: 'table',
									label: 'Table',
									icon: 'umb:table',
								},
								{
									alias: 'hr',
									label: 'Horizontal rule',
									icon: 'umb:horizontal-rule',
								},
								{
									alias: 'subscript',
									label: 'Subscript',
									icon: 'umb:subscript',
								},
								{
									alias: 'superscript',
									label: 'Superscript',
									icon: 'umb:superscript',
								},
								{
									alias: 'charmap',
									label: 'Character map',
									icon: 'umb:character-map',
								},
								{
									alias: 'rtl',
									label: 'Right to left',
									icon: 'umb:format-textdirection-r-to-l',
								},
								{
									alias: 'ltr',
									label: 'Left to right',
									icon: 'umb:format-textdirection-l-to-r',
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
