import type { ManifestPropertyEditorSchema } from '@umbraco-cms/backoffice/property-editor';

export const manifest: ManifestPropertyEditorSchema = {
	type: 'propertyEditorSchema',
	name: 'Checkbox List',
	alias: 'Umbraco.CheckBoxList',
	meta: {
		defaultPropertyEditorUiAlias: 'Umb.PropertyEditorUi.CheckBoxList',
	},
};
