import type { ManifestPropertyEditorSchema } from '@umbraco-cms/backoffice/property-editor';

export const manifest: ManifestPropertyEditorSchema = {
	type: 'propertyEditorSchema',
	name: 'Dropdown',
	alias: 'Umbraco.DropDown.Flexible',
	meta: {
		defaultPropertyEditorUiAlias: 'Umb.PropertyEditorUi.Dropdown',
		settings: {
			properties: [
				{
					alias: 'multiple',
					label: 'Enable multiple choice',
					propertyEditorUiAlias: 'Umb.PropertyEditorUi.Toggle',
				},
				{
					alias: 'items',
					label: 'Add options',
					propertyEditorUiAlias: 'Umb.PropertyEditorUi.MultipleTextString',
				},
			],
		},
	},
};
