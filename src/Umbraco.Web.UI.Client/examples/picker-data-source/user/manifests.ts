export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'propertyEditorDataSource',
		dataSourceType: 'picker',
		alias: 'Umb.PropertyEditorDataSource.UserPicker',
		name: 'User Picker Data Source',
		api: () => import('./example-user-picker-data-source.js'),
		meta: {
			label: 'Users',
			icon: 'icon-user',
			description: 'Pick a user',
		},
	},
];
