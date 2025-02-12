import { manifest as blockRteTypeManifest } from './block/manifests.js';
import { manifests as tinyMceManifest } from './tiny-mce/manifests.js';

export const manifests: Array<UmbExtensionManifest> = [
	...tinyMceManifest,
	blockRteTypeManifest,
	{
		type: 'propertyEditorUi',
		alias: 'Umb.PropertyEditorUI.TinyMCE.ToolbarConfiguration',
		name: 'TinyMCE Toolbar Property Editor UI',
		element: () => import('./toolbar/property-editor-ui-tiny-mce-toolbar-configuration.element.js'),
		meta: {
			label: 'TinyMCE Toolbar Configuration',
			icon: 'icon-autofill',
			group: 'common',
		},
	},
	{
		type: 'propertyEditorUi',
		alias: 'Umb.PropertyEditorUI.TinyMCE.StylesheetsConfiguration',
		name: 'TinyMCE Stylesheets Property Editor UI',
		element: () => import('./stylesheets/property-editor-ui-tiny-mce-stylesheets-configuration.element.js'),
		meta: {
			label: 'TinyMCE Stylesheets Configuration',
			icon: 'icon-autofill',
			group: 'common',
		},
	},
	{
		type: 'propertyEditorUi',
		alias: 'Umb.PropertyEditorUI.TinyMCE.DimensionsConfiguration',
		name: 'TinyMCE Dimensions Property Editor UI',
		element: () => import('./dimensions/property-editor-ui-tiny-mce-dimensions-configuration.element.js'),
		meta: {
			label: 'TinyMCE Dimensions Configuration',
			icon: 'icon-autofill',
			group: 'common',
		},
	},
	{
		type: 'propertyEditorUi',
		alias: 'Umb.PropertyEditorUI.TinyMCE.MaxImageSizeConfiguration',
		name: 'TinyMCE Max Image Size Property Editor UI',
		element: () => import('./max-image-size/property-editor-ui-tiny-mce-maximagesize.element.js'),
		meta: {
			label: 'TinyMCE Max Image Size Configuration',
			icon: 'icon-autofill',
			group: 'common',
		},
	},
];
