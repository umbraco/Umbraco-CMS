export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'propertyEditorSchema',
		name: 'Decimal',
		alias: 'Umbraco.Decimal',
		meta: {
			defaultPropertyEditorUiAlias: 'Umb.PropertyEditorUi.Decimal',
			settings: {
				properties: [
					{
						alias: 'validationRange',
						label: 'Value range',
						description: 'Set the minimum and maximum value that can be entered',
						propertyEditorUiAlias: 'Umb.PropertyEditorUi.NumberRange',
						config: [{ alias: 'step', value: 0.000001 }],
					},
					{
						alias: 'step',
						label: 'Step size',
						description: 'Enter the intervals amount between each step of number to be entered',
						propertyEditorUiAlias: 'Umb.PropertyEditorUi.Decimal',
						config: [{ alias: 'step', value: '0.000001' }],
					},
				],
			},
		},
	},
];
