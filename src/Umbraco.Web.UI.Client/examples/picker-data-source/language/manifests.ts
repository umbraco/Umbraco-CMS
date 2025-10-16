export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'propertyEditorDataSource',
		dataSourceType: 'picker',
		alias: 'Umb.PropertyEditorDataSource.LanguagePicker',
		name: 'Language Picker Data Source',
		api: () => import('./example-language-picker-data-source.js'),
		meta: {
			label: 'Languages',
			icon: 'icon-globe',
			description: 'Pick a language',
		},
	},
];
