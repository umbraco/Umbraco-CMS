import type { ManifestPropertyEditorConfigConverter } from '@umbraco-cms/backoffice/property-editor';

const fromPropertyEditorSchemaAlias = 'Umbraco.MultiNodeTreePicker';

export const manifests: Array<ManifestPropertyEditorConfigConverter> = [
	{
		type: 'propertyEditorConfigConverter',
		alias: 'Umb.PropertyEditorConfigConverter.ContentPicker.DocumentPicker.Single',
		name: 'Multi Node Tree Picker to Single Document Picker Config Converter',
		api: () => import('./content-picker-to-document-picker-config-converter.js'),
		fromPropertyEditorSchemaAlias,
		forPropertyEditorSchemaAlias: 'Umbraco.ContentPicker',
	},
	{
		type: 'propertyEditorConfigConverter',
		alias: 'Umb.PropertyEditorConfigConverter.ContentPicker.DocumentPicker.Multiple',
		name: 'Multi Node Tree Picker to Document Picker Config Converter',
		api: () => import('./content-picker-to-document-picker-config-converter.js'),
		fromPropertyEditorSchemaAlias,
		forPropertyEditorSchemaAlias: 'Umbraco.DocumentPicker.Multiple',
	},
	{
		type: 'propertyEditorConfigConverter',
		alias: 'Umb.PropertyEditorConfigConverter.ContentPicker.MediaPicker.Single',
		name: 'Multi Node Tree Picker to Single Media Picker Config Converter',
		api: () => import('./content-picker-to-media-picker-config-converter.js'),
		fromPropertyEditorSchemaAlias,
		forPropertyEditorSchemaAlias: 'Umbraco.MediaPicker.Single',
	},
	{
		type: 'propertyEditorConfigConverter',
		alias: 'Umb.PropertyEditorConfigConverter.ContentPicker.MediaPicker.Multiple',
		name: 'Multi Node Tree Picker to Media Picker Config Converter',
		api: () => import('./content-picker-to-media-picker-config-converter.js'),
		fromPropertyEditorSchemaAlias,
		forPropertyEditorSchemaAlias: 'Umbraco.MediaPicker3',
	},
	{
		type: 'propertyEditorConfigConverter',
		alias: 'Umb.PropertyEditorConfigConverter.ContentPicker.MemberPicker.Single',
		name: 'Multi Node Tree Picker to Single Member Picker Config Converter',
		api: () => import('./content-picker-to-member-picker-config-converter.js'),
		fromPropertyEditorSchemaAlias,
		forPropertyEditorSchemaAlias: 'Umbraco.MemberPicker',
	},
	{
		type: 'propertyEditorConfigConverter',
		alias: 'Umb.PropertyEditorConfigConverter.ContentPicker.MemberPicker.Multiple',
		name: 'Multi Node Tree Picker to Member Picker Config Converter',
		api: () => import('./content-picker-to-member-picker-config-converter.js'),
		fromPropertyEditorSchemaAlias,
		forPropertyEditorSchemaAlias: 'Umbraco.MemberPicker.Multiple',
	},
];
