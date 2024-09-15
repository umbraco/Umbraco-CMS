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
						alias: 'min',
						label: 'Minimum',
						description: 'Enter the minimum amount of text boxes to be displayed',
						propertyEditorUiAlias: 'Umb.PropertyEditorUi.Integer',
						config: [{ alias: 'min', value: 0 }],
					},
					{
						alias: 'max',
						label: 'Maximum',
						description: 'Enter the maximum amount of text boxes to be displayed, enter 0 for unlimited',
						propertyEditorUiAlias: 'Umb.PropertyEditorUi.Integer',
						config: [{ alias: 'min', value: 0 }],
					},
				],
				defaultData: [
					{ alias: 'min', value: 0 },
					{ alias: 'max', value: 0 },
				],
			},
		},
	},
];
