export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'propertyEditorSchema',
		name: 'Entity Data Picker',
		alias: 'Umbraco.EntityDataPicker',
		meta: {
			defaultPropertyEditorUiAlias: 'Umb.PropertyEditorUi.EntityDataPicker',
			settings: {
				properties: [
					{
						alias: 'validationLimit',
						label: 'Amount',
						propertyEditorUiAlias: 'Umb.PropertyEditorUi.NumberRange',
						config: [{ alias: 'validationRange', value: { min: 0, max: Infinity } }],
						weight: 100,
					},
				],
			},
		},
	},
];
