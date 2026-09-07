import { UMB_PICKER_DATA_SOURCE_TYPE } from '@umbraco-cms/backoffice/picker-data-source';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'propertyEditorDataSource',
		dataSourceType: UMB_PICKER_DATA_SOURCE_TYPE,
		alias: 'Umb.PropertyEditorDataSource.CustomWithSearchPickerCollection',
		name: 'Custom With Search Picker Collection Data Source',
		api: () => import('./example-custom-with-search-picker-collection-data-source.js'),
		meta: {
			label: 'Example Items With Search (Collection)',
			icon: 'icon-list',
			description: 'Pick example items from a collection that supports search but no text filter',
		},
	},
];
