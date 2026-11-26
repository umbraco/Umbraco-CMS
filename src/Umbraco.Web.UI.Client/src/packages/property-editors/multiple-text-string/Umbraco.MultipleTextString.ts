export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'propertyEditorSchema',
		name: 'Multiple Text String',
		alias: 'Umbraco.MultipleTextstring',
		meta: {
			defaultPropertyEditorUiAlias: 'Umb.PropertyEditorUi.MultipleTextString',
			settings: {
				properties: [
					{
						alias: 'validationLimit',
						label: 'Amount',
						description: '{#validation_minimumOnlyWhenInUse}',
						propertyEditorUiAlias: 'Umb.PropertyEditorUi.NumberRange',
						config: [{ alias: 'validationRange', value: { min: 0, max: Infinity } }],
					},
				],
			},
		},
	},
];
