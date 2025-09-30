import { manifests as collectionManifests } from './collection/manifests.js';
import { manifests as itemManifests } from './item/manifests.js';
import { manifests as searchManifests } from './search/manifests.js';

export const manifests: Array<UmbExtensionManifest> = [
	...collectionManifests,
	...itemManifests,
	...searchManifests,
	{
		type: 'pickerPropertyEditorTreeDataSource',
		alias: 'Umb.PickerPropertyEditorTreeDataSource.Document',
		name: 'Document Picker Tree Data Source',
		api: () => import('./test-document-picker-data-source.js'),
		meta: {
			label: 'Documents',
			icon: 'icon-document',
			description: 'Pick a document',
			settings: {
				properties: [
					{
						alias: 'testSetting',
						label: 'From Document Data Source',
						propertyEditorUiAlias: 'Umb.PropertyEditorUi.TextBox',
					},
				],
			},
		},
	},
	{
		type: 'pickerPropertyEditorTreeDataSource',
		alias: 'Umb.PickerPropertyEditorTreeDataSource.Media',
		name: 'Media Picker Tree Data Source',
		api: () => import('./test-media-picker-data-source.js'),
		meta: {
			label: 'Media',
			icon: 'icon-document-image',
			description: 'Pick a media item',
		},
	},
	{
		type: 'pickerPropertyEditorCollectionDataSource',
		alias: 'Umb.PickerPropertyEditorCollectionDataSource.Language',
		name: 'Language Picker Collection Data Source',
		api: () => import('./test-language-picker-data-source.js'),
		meta: {
			label: 'Languages',
			icon: 'icon-globe',
			description: 'Pick a language',
		},
	},
	{
		type: 'pickerPropertyEditorCollectionDataSource',
		alias: 'Umb.PickerPropertyEditorCollectionDataSource.Webhook',
		name: 'Webhook Picker Collection Data Source',
		api: () => import('./test-webhook-picker-data-source.js'),
		meta: {
			label: 'Webhooks',
			icon: 'icon-webhook',
			description: 'Pick a webhook',
		},
	},
];
