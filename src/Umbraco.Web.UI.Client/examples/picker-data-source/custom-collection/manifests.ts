export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'propertyEditorDataSource',
		dataSourceType: 'picker',
		alias: 'Umb.PropertyEditorDataSource.CustomPickerCollection',
		name: 'Custom Picker Collection Data Source',
		api: () => import('./example-custom-picker-collection-data-source.js'),
		meta: {
			label: 'Example Items (Collection)',
			icon: 'icon-list',
			description: 'Pick example items from a collection',
		},
	},
];
