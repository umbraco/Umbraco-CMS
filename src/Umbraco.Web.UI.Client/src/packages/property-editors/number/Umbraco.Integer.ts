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
						alias: 'validationRange',
						label: 'Value range',
						description: 'Set the minimum and maximum value that can be entered',
						propertyEditorUiAlias: 'Umb.PropertyEditorUi.NumberRange',
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
