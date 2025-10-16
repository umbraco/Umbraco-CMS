export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'propertyEditorDataSource',
		dataSourceType: 'picker',
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
						alias: 'filter',
						label: 'Allow items of type',
						description: 'Select the applicable types',
						propertyEditorUiAlias: 'Umb.PropertyEditorUi.ContentPicker.SourceType',
					},
				],
			},
		},
	},
];
