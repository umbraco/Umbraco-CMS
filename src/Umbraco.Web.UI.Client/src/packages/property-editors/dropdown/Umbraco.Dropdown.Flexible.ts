import type { ManifestPropertyEditorSchema } from '@umbraco-cms/backoffice/property-editor';

export const manifest: ManifestPropertyEditorSchema = {
	type: 'propertyEditorSchema',
	name: 'Select List',
	alias: 'Umbraco.DropDown.Flexible',
	meta: {
		defaultPropertyEditorUiAlias: 'Umb.PropertyEditorUi.Dropdown',
		settings: {
			properties: [
				{
					alias: 'items',
					label: 'Add options',
					propertyEditorUiAlias: 'Umb.PropertyEditorUi.MultipleTextString',
				},
			],
		},
	},
};
