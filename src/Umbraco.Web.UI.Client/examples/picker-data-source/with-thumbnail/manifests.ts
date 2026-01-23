import { UMB_PICKER_DATA_SOURCE_TYPE } from '@umbraco-cms/backoffice/picker-data-source';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'propertyEditorDataSource',
		dataSourceType: UMB_PICKER_DATA_SOURCE_TYPE,
		alias: 'Umb.PropertyEditorDataSource.CustomWithThumbnailPickerCollection',
		name: 'Custom With Thumbnail Picker Collection Data Source',
		api: () => import('./example-custom-with-thumbnail-picker-collection-data-source.js'),
		meta: {
			label: 'Example Items With Thumbnails (Collection)',
			icon: 'icon-list',
			description: 'Pick example items with thumbnails from a collection',
		},
	},
];
