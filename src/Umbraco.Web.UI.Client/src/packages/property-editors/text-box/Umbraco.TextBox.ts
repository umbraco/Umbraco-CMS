import type { ManifestPropertyEditorSchema } from '@umbraco-cms/backoffice/property-editor';

export const manifest: ManifestPropertyEditorSchema = {
	type: 'propertyEditorSchema',
	name: 'Text Box',
	alias: 'Umbraco.TextBox',
	meta: {
		defaultPropertyEditorUiAlias: 'Umb.PropertyEditorUi.TextBox',
		settings: {
			properties: [
				{
					alias: 'maxChars',
					label: 'Maximum allowed characters',
					propertyEditorUiAlias: 'Umb.PropertyEditorUi.Integer',
					config: [
						{ alias: 'min', value: 1 },
						{ alias: 'max', value: 512 },
						{ alias: 'placeholder', value: '512' },
					],
				},
				{
					alias: 'autocomplete',
					label: 'Autocomplete',
					description: 'Controls browser autocomplete behaviour',
					propertyEditorUiAlias: 'Umb.PropertyEditorUi.Dropdown',
					config: [
						{
							alias: 'items',
							value: [
								{ name: 'On', value: 'on' },
								{ name: 'Off', value: 'off' },
							],
						},
					],
				},
				{
					alias: 'placeholder',
					label: 'Placeholder',
					description: 'Placeholder text shown inside the input when empty',
					propertyEditorUiAlias: 'Umb.PropertyEditorUi.TextBox',
				},
			],
			defaultData: [
				{
					alias: 'maxChars',
					value: 512,
				}
			],
		},
	},
};
