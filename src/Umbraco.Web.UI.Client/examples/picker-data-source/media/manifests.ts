export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'propertyEditorDataSource',
		dataSourceType: 'picker',
		alias: 'Umb.PropertyEditorDataSource.MediaPicker',
		name: 'Media Picker Data Source',
		api: () => import('./example-media-picker-data-source.js'),
		meta: {
			label: 'Media',
			icon: 'icon-document-image',
			description: 'Pick a media item',
		},
	},
];
