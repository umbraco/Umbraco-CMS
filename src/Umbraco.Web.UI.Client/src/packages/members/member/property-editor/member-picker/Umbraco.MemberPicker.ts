import type { ManifestPropertyEditorSchema } from '@umbraco-cms/backoffice/property-editor';

export const manifest: ManifestPropertyEditorSchema = {
	type: 'propertyEditorSchema',
	name: 'Member Picker',
	alias: 'Umbraco.MemberPicker',
	meta: {
		defaultPropertyEditorUiAlias: 'Umb.PropertyEditorUi.MemberPicker',
		settings: {
			properties: [
				{
					alias: 'filter',
					label: 'Accepted types',
					description: 'Limit to specific member types',
					propertyEditorUiAlias: 'Umb.PropertyEditorUi.MemberTypePicker',
					weight: 10,
				},
			],
		},
	},
};
