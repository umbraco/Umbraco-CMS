import { manifest as schemaManifest } from './Umbraco.MediaPicker.js';
import { manifests as valueSummaryManifests } from './value-summary/manifests.js';

const propertyEditorUiAlias = 'Umb.PropertyEditorUi.MediaPicker';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'propertyEditorUi',
		alias: propertyEditorUiAlias,
		name: 'Media Picker Property Editor UI',
		element: () => import('./property-editor-ui-media-picker.element.js'),
		meta: {
			supportsVariantChange: true,
			label: 'Media Picker',
			propertyEditorSchemaAlias: 'Umbraco.MediaPicker3',
			icon: 'icon-picture',
			group: '#propertyEditorUIGroups_media',
			keywords: [
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
			],
			supportsReadOnly: true,
		},
	},
	schemaManifest,
	...valueSummaryManifests,
	{
		type: 'propertyAction',
		kind: 'clear',
		alias: 'Umb.PropertyAction.MediaPicker.Clear',
		name: 'Clear Media Picker Property Action',
		forPropertyEditorUis: [propertyEditorUiAlias],
	},
];
