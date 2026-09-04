import type { ManifestPropertyEditorSchema } from '@umbraco-cms/backoffice/property-editor';

export const manifest: ManifestPropertyEditorSchema = {
	type: 'propertyEditorSchema',
	name: 'Multiple Member Picker',
	alias: 'Umbraco.MultipleMemberPicker',
	meta: {
		defaultPropertyEditorUiAlias: 'Umb.PropertyEditorUi.MultipleMemberPicker',
		settings: {
			properties: [
				{
					alias: 'filter',
					label: 'Accepted types',
					description: 'Limit to specific member types',
					propertyEditorUiAlias: 'Umb.PropertyEditorUi.MemberTypePicker',
					weight: 10,
				},
				{
					alias: 'validationLimit',
					label: 'Amount',
					description: 'Set a required range of members',
					propertyEditorUiAlias: 'Umb.PropertyEditorUi.NumberRange',
					config: [{ alias: 'validationRange', value: { min: 0, max: Infinity } }],
					weight: 20,
				},
			],
		},
	},
};
