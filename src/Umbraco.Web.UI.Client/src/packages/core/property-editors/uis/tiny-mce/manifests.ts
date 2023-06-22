import { manifests as configuration } from './config/manifests.js';
import type { ManifestPropertyEditorUi } from '@umbraco-cms/backoffice/extension-registry';

const manifest: ManifestPropertyEditorUi = {
	type: 'propertyEditorUi',
	alias: 'Umb.PropertyEditorUi.TinyMCE',
	name: 'Rich Text Editor Property Editor UI',
	loader: () => import('./property-editor-ui-tiny-mce.element.js'),
	meta: {
		label: 'Rich Text Editor',
		propertyEditorAlias: 'Umbraco.RichText',
		icon: 'umb:browser-window',
		group: 'richText',
		settings: {
			properties: [
				{
					alias: 'editor',
					label: 'Editor',
					propertyEditorUiAlias: 'Umb.PropertyEditorUi.TinyMCE.Configuration',
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
			],
		},
	},
};

export const manifests = [manifest, ...configuration];
