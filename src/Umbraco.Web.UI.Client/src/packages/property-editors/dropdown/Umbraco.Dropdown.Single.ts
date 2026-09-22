import type { ManifestPropertyEditorSchema } from '@umbraco-cms/backoffice/property-editor';

export const manifest: ManifestPropertyEditorSchema = {
	type: 'propertyEditorSchema',
	name: 'Single Dropdown',
	alias: 'Umbraco.DropDown.Single',
	meta: {
		defaultPropertyEditorUiAlias: 'Umb.PropertyEditorUi.Dropdown.Single',
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
