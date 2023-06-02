import type { ManifestPropertyEditorUi } from '@umbraco-cms/backoffice/extension-registry';

const configurationManifests: Array<ManifestPropertyEditorUi> = [
	{
		type: 'propertyEditorUi',
		alias: 'Umb.PropertyEditorUI.TinyMCE.ToolbarConfiguration',
		name: 'TinyMCE Toolbar Property Editor UI',
		loader: () => import('./toolbar/property-editor-ui-tiny-mce-toolbar-configuration.element.js'),
		meta: {
			label: 'TinyMCE Toolbar Configuration',
			propertyEditorAlias: 'Umbraco.TinyMCE.Configuration',
			icon: 'umb:autofill',
			group: 'common',
		},
	},
	{
		type: 'propertyEditorUi',
		alias: 'Umb.PropertyEditorUI.TinyMCE.StylesheetsConfiguration',
		name: 'TinyMCE Stylesheets Property Editor UI',
		loader: () => import('./stylesheets/property-editor-ui-tiny-mce-stylesheets-configuration.element.js'),
		meta: {
			label: 'TinyMCE Stylesheets Configuration',
			propertyEditorAlias: 'Umbraco.TinyMCE.Configuration',
			icon: 'umb:autofill',
			group: 'common',
		},
	},
	{
		type: 'propertyEditorUi',
		alias: 'Umb.PropertyEditorUI.TinyMCE.DimensionsConfiguration',
		name: 'TinyMCE Dimensions Property Editor UI',
		loader: () => import('./dimensions/property-editor-ui-tiny-mce-dimensions-configuration.element.js'),
		meta: {
			label: 'TinyMCE Dimensions Configuration',
			propertyEditorAlias: 'Umbraco.TinyMCE.Configuration',
			icon: 'umb:autofill',
			group: 'common',
		},
	},	
	{
		type: 'propertyEditorUi',
		alias: 'Umb.PropertyEditorUI.TinyMCE.MaxImageSizeConfiguration',
		name: 'TinyMCE Max Image Size Property Editor UI',
		loader: () => import('./max-image-size/property-editor-ui-tiny-mce-maximagesize-configuration.element.js'),
		meta: {
			label: 'TinyMCE Max Image Size Configuration',
			propertyEditorAlias: 'Umbraco.TinyMCE.Configuration',
			icon: 'umb:autofill',
			group: 'common',
		},
	},	
];

export const manifests = [...configurationManifests];