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
					label: '#textbox_maxCharsLabel',
					propertyEditorUiAlias: 'Umb.PropertyEditorUi.Integer',
					config: [
						{ alias: 'min', value: 1 },
						{ alias: 'max', value: 512 },
						{ alias: 'placeholder', value: '512' },
					],
				},
				{
					alias: 'autocomplete',
					label: '#textbox_autocompleteLabel',
					propertyEditorUiAlias: 'Umb.PropertyEditorUi.Dropdown',
					config: [
						{
							alias: 'items',
							value: [
								{ name: 'On', value: 'on' },
								{ name: 'Off', value: 'off' },
							],
						},
						{
							alias: 'placeholder',
							value: '#general_placeholder',
						},
					],
				},
				{
					alias: 'placeholder',
					label: '#general_placeholder',
					description: 'Placeholder text shown inside the input when empty',
					propertyEditorUiAlias: 'Umb.PropertyEditorUi.TextBox',
				},
			],
			defaultData: [
				{
					alias: 'maxChars',
					value: 512,
				},
				{
					alias: 'autocomplete',
					value: 'off',
				},
			],
		},
	},
};
