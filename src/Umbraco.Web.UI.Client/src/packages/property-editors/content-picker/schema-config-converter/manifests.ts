import type { ManifestPropertyEditorSchemaConfigConverter } from '@umbraco-cms/backoffice/property-editor';

const fromPropertyEditorSchemaAlias = 'Umbraco.MultiNodeTreePicker';

export const manifests: Array<ManifestPropertyEditorSchemaConfigConverter> = [
	{
		type: 'propertyEditorSchemaConfigConverter',
		alias: 'Umb.PropertyEditorSchemaConfigConverter.ContentPicker.DocumentPicker.Single',
		name: 'Multi Node Tree Picker to Single Document Picker Config Converter',
		api: () => import('./content-picker-to-document-picker-schema-config-converter.js'),
		fromPropertyEditorSchemaAlias,
		forPropertyEditorSchemaAlias: 'Umbraco.ContentPicker',
	},
	{
		type: 'propertyEditorSchemaConfigConverter',
		alias: 'Umb.PropertyEditorSchemaConfigConverter.ContentPicker.DocumentPicker.Multiple',
		name: 'Multi Node Tree Picker to Document Picker Config Converter',
		api: () => import('./content-picker-to-document-picker-schema-config-converter.js'),
		fromPropertyEditorSchemaAlias,
		forPropertyEditorSchemaAlias: 'Umbraco.DocumentPicker.Multiple',
	},
	{
		type: 'propertyEditorSchemaConfigConverter',
		alias: 'Umb.PropertyEditorSchemaConfigConverter.ContentPicker.MediaPicker.Single',
		name: 'Multi Node Tree Picker to Single Media Picker Config Converter',
		api: () => import('./content-picker-to-media-picker-schema-config-converter.js'),
		fromPropertyEditorSchemaAlias,
		forPropertyEditorSchemaAlias: 'Umbraco.MediaPicker.Single',
	},
	{
		type: 'propertyEditorSchemaConfigConverter',
		alias: 'Umb.PropertyEditorSchemaConfigConverter.ContentPicker.MediaPicker.Multiple',
		name: 'Multi Node Tree Picker to Media Picker Config Converter',
		api: () => import('./content-picker-to-media-picker-schema-config-converter.js'),
		fromPropertyEditorSchemaAlias,
		forPropertyEditorSchemaAlias: 'Umbraco.MediaPicker3',
	},
	{
		type: 'propertyEditorSchemaConfigConverter',
		alias: 'Umb.PropertyEditorSchemaConfigConverter.ContentPicker.MemberPicker.Single',
		name: 'Multi Node Tree Picker to Single Member Picker Config Converter',
		api: () => import('./content-picker-to-member-picker-schema-config-converter.js'),
		fromPropertyEditorSchemaAlias,
		forPropertyEditorSchemaAlias: 'Umbraco.MemberPicker',
	},
	{
		type: 'propertyEditorSchemaConfigConverter',
		alias: 'Umb.PropertyEditorSchemaConfigConverter.ContentPicker.MemberPicker.Multiple',
		name: 'Multi Node Tree Picker to Member Picker Config Converter',
		api: () => import('./content-picker-to-member-picker-schema-config-converter.js'),
		fromPropertyEditorSchemaAlias,
		forPropertyEditorSchemaAlias: 'Umbraco.MemberPicker.Multiple',
	},
];
