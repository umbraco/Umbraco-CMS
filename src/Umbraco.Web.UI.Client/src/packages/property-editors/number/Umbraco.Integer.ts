export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'propertyEditorSchema',
		name: 'Integer',
		alias: 'Umbraco.Integer',
		meta: {
			defaultPropertyEditorUiAlias: 'Umb.PropertyEditorUi.Integer',
			settings: {
				properties: [
					{
						alias: 'min',
						label: 'Minimum',
						description: 'Enter the minimum amount of number to be entered',
						propertyEditorUiAlias: 'Umb.PropertyEditorUi.Integer',
					},
					{
						alias: 'max',
						label: 'Maximum',
						description: 'Enter the maximum amount of number to be entered',
						propertyEditorUiAlias: 'Umb.PropertyEditorUi.Integer',
						config: [{ alias: 'placeholder', value: '∞' }],
					},
					{
						alias: 'step',
						label: 'Step size',
						description: 'Enter the intervals amount between each step of number to be entered',
						propertyEditorUiAlias: 'Umb.PropertyEditorUi.Integer',
					},
				],
			},
		},
	},
];
