import { manifest as schemaManifest } from './Umbraco.MediaPicker.js';
import { manifest as singleSchemaManifest } from './Umbraco.SingleMediaPicker.js';
import { manifests as valueSummaryManifests } from './value-summary/manifests.js';

const keywords = [
	'select',
	'image',
	'photo',
	'picture',
	'banner',
	'thumbnail',
	'logo',
	'avatar',
	'gallery',
	'media',
	'video',
	'file',
	'attachment',
	'cover',
];

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'propertyEditorUi',
		alias: 'Umb.PropertyEditorUi.MediaPicker',
		name: 'Media Picker Property Editor UI',
		element: () => import('./property-editor-ui-media-picker.element.js'),
		meta: {
			label: 'Media Picker',
			propertyEditorSchemaAlias: 'Umbraco.MediaPicker3',
			icon: 'icon-pictures',
			group: '#propertyEditorUIGroups_media',
			keywords: [...keywords, 'multiple', 'several'],
			supportsReadOnly: true,
		},
	},
	{
		type: 'propertyEditorUi',
		alias: 'Umb.PropertyEditorUi.SingleMediaPicker',
		name: 'Single Media Picker Property Editor UI',
		element: () => import('./property-editor-ui-single-media-picker.element.js'),
		meta: {
			label: 'Single Media Picker',
			propertyEditorSchemaAlias: 'Umbraco.SingleMediaPicker',
			icon: 'icon-picture',
			group: '#propertyEditorUIGroups_media',
			keywords: [...keywords, 'single', 'one'],
			supportsReadOnly: true,
		},
	},
	schemaManifest,
	singleSchemaManifest,
	...valueSummaryManifests,
];
