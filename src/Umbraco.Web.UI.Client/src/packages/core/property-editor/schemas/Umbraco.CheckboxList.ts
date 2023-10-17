import type { ManifestPropertyEditorSchema } from '@umbraco-cms/backoffice/extension-registry';

export const manifest: ManifestPropertyEditorSchema = {
	type: 'propertyEditorSchema',
	name: 'Checkbox List',
	alias: 'Umbraco.CheckBoxList',
	meta: {
		defaultPropertyEditorUiAlias: 'Umb.PropertyEditorUi.CheckboxList',
	},
};
