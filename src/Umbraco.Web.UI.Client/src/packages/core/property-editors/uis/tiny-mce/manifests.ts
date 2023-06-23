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
				{
					alias: 'imageUploadFolder',
					label: 'Image Upload Folder',
					description: 'Choose the upload location of pasted images',
					propertyEditorUiAlias: 'Umb.PropertyEditorUi.MediaPicker',
					config: [
						{
							alias: 'validationLimit',
							value: {
								min: 1,
								max: 1,
							},
						},
					],
				},
			],
		},
	},
};

export const manifests = [manifest, ...configuration];
