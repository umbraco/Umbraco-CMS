import { manifests as collectionManifests } from './collection/manifests.js';
import { manifests as itemManifests } from './item/manifests.js';
import { manifests as searchManifests } from './search/manifests.js';

export const manifests: Array<UmbExtensionManifest> = [
	...collectionManifests,
	...itemManifests,
	...searchManifests,
	{
		type: 'propertyEditorDataSource',
		dataSourceType: 'pickerTree',
		alias: 'Umb.PropertyEditorDataSource.DocumentPicker',
		name: 'Document Picker Tree Data Source',
		api: () => import('./test-document-picker-data-source.js'),
		meta: {
			label: 'Documents',
			icon: 'icon-document',
			description: 'Pick a document',
			settings: {
				properties: [
					{
						alias: 'startNode',
						label: 'Node type',
						description: '',
						propertyEditorUiAlias: 'Umb.PropertyEditorUi.ContentPicker.Source',
					},
					{
						alias: 'filter',
						label: 'Allow items of type',
						description: 'Select the applicable types',
						propertyEditorUiAlias: 'Umb.PropertyEditorUi.ContentPicker.SourceType',
					},
				],
			},
		},
	},
	{
		type: 'propertyEditorDataSource',
		dataSourceType: 'pickerTree',
		alias: 'Umb.PropertyEditorDataSource.MediaPicker',
		name: 'Media Picker Tree Data Source',
		api: () => import('./test-media-picker-data-source.js'),
		meta: {
			label: 'Media',
			icon: 'icon-document-image',
			description: 'Pick a media item',
		},
	},
	{
		type: 'propertyEditorDataSource',
		dataSourceType: 'pickerCollection',
		alias: 'Umb.PropertyEditorDataSource.LanguagePicker',
		name: 'Language Picker Collection Data Source',
		api: () => import('./test-language-picker-data-source.js'),
		meta: {
			label: 'Languages',
			icon: 'icon-globe',
			description: 'Pick a language',
		},
	},
	{
		type: 'propertyEditorDataSource',
		dataSourceType: 'pickerCollection',
		alias: 'Umb.PropertyEditorDataSource.WebhookPicker',
		name: 'Webhook Picker Data Source',
		api: () => import('./test-webhook-picker-data-source.js'),
		meta: {
			label: 'Webhooks',
			icon: 'icon-webhook',
			description: 'Pick a webhook',
		},
	},
	{
		type: 'propertyEditorDataSource',
		dataSourceType: 'pickerCollection',
		alias: 'Umb.PropertyEditorDataSource.UserPicker',
		name: 'User Picker Data Source',
		api: () => import('./test-user-picker-data-source.js'),
		meta: {
			label: 'Users',
			icon: 'icon-user',
			description: 'Pick a user',
		},
	},
];
