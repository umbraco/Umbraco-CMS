export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'propertyEditorDataSource',
		dataSourceType: 'picker',
		alias: 'Umb.PropertyEditorDataSource.CustomPickerTree',
		name: 'Custom Picker Tree Data Source',
		api: () => import('./example-custom-picker-tree-data-source.js'),
		meta: {
			label: 'Example Items (Tree)',
			icon: 'icon-tree',
			description: 'Pick example items from a tree',
		},
	},
];
