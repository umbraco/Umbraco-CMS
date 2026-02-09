import { manifests as withThumbnailManifests } from './with-thumbnail/manifests.js';
import { UMB_PICKER_DATA_SOURCE_TYPE } from '@umbraco-cms/backoffice/picker-data-source';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'propertyEditorDataSource',
		dataSourceType: UMB_PICKER_DATA_SOURCE_TYPE,
		alias: 'Umb.PropertyEditorDataSource.CustomPickerCollection',
		name: 'Custom Picker Collection Data Source',
		api: () => import('./example-custom-picker-collection-data-source.js'),
		meta: {
			label: 'Example Items (Collection)',
			icon: 'icon-list',
			description: 'Pick example items from a collection',
		},
	},
	{
		type: 'propertyEditorDataSource',
		dataSourceType: UMB_PICKER_DATA_SOURCE_TYPE,
		alias: 'Umb.PropertyEditorDataSource.CustomPickerTree',
		name: 'Custom Picker Tree Data Source',
		api: () => import('./example-custom-picker-tree-data-source.js'),
		meta: {
			label: 'Example Items (Tree)',
			icon: 'icon-tree',
			description: 'Pick example items from a tree',
		},
	},
	{
		type: 'propertyEditorDataSource',
		dataSourceType: UMB_PICKER_DATA_SOURCE_TYPE,
		alias: 'Umb.PropertyEditorDataSource.DocumentPicker',
		name: 'Document Picker Data Source',
		api: () => import('./example-document-picker-data-source.js'),
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
		dataSourceType: UMB_PICKER_DATA_SOURCE_TYPE,
		alias: 'Umb.PropertyEditorDataSource.MediaPicker',
		name: 'Media Picker Data Source',
		api: () => import('./example-media-picker-data-source.js'),
		meta: {
			label: 'Media',
			icon: 'icon-document-image',
			description: 'Pick a media item',
		},
	},
	{
		type: 'propertyEditorDataSource',
		dataSourceType: UMB_PICKER_DATA_SOURCE_TYPE,
		alias: 'Umb.PropertyEditorDataSource.LanguagePicker',
		name: 'Language Picker Data Source',
		api: () => import('./example-language-picker-data-source.js'),
		meta: {
			label: 'Languages',
			icon: 'icon-globe',
			description: 'Pick a language',
		},
	},
	{
		type: 'propertyEditorDataSource',
		dataSourceType: UMB_PICKER_DATA_SOURCE_TYPE,
		alias: 'Umb.PropertyEditorDataSource.WebhookPicker',
		name: 'Webhook Picker Data Source',
		api: () => import('./example-webhook-picker-data-source.js'),
		meta: {
			label: 'Webhooks',
			icon: 'icon-webhook',
			description: 'Pick a webhook',
		},
	},
	{
		type: 'propertyEditorDataSource',
		dataSourceType: UMB_PICKER_DATA_SOURCE_TYPE,
		alias: 'Umb.PropertyEditorDataSource.UserPicker',
		name: 'User Picker Data Source',
		api: () => import('./example-user-picker-data-source.js'),
		meta: {
			label: 'Users',
			icon: 'icon-user',
			description: 'Pick a user',
		},
	},
	...withThumbnailManifests,
];
