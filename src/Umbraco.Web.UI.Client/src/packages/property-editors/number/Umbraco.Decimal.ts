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
						alias: 'min',
						label: 'Minimum',
						description: 'Enter the minimum amount of number to be entered',
						propertyEditorUiAlias: 'Umb.PropertyEditorUi.Decimal',
					},
					{
						alias: 'max',
						label: 'Maximum',
						description: 'Enter the maximum amount of number to be entered',
						propertyEditorUiAlias: 'Umb.PropertyEditorUi.Decimal',
					},
					{
						alias: 'step',
						label: 'Step size',
						description: 'Enter the intervals amount between each step of number to be entered',
						propertyEditorUiAlias: 'Umb.PropertyEditorUi.Decimal',
						config: [
							{
								alias: 'step',
								value: '0.01',
							},
						],
					},
				],
			},
		},
	},
];
