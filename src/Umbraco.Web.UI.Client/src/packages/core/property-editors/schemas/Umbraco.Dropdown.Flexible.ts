import type { ManifestPropertyEditorSchema } from '@umbraco-cms/backoffice/extension-registry';

// TODO: We won't include momentjs anymore so we need to find a way to handle date formats
export const manifest: ManifestPropertyEditorSchema = {
	type: 'propertyEditorSchema',
	name: 'Dropdown',
	alias: 'Umbraco.DropDown.Flexible',
	meta: {
		defaultPropertyEditorUiAlias: 'Umb.PropertyEditorUi.DropDown.Flexible',
	},
};
