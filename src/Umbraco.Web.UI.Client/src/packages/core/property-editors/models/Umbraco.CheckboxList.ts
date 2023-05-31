import type { ManifestPropertyEditorModel } from '@umbraco-cms/backoffice/extension-registry';

export const manifest: ManifestPropertyEditorModel = {
	type: 'propertyEditorModel',
	name: 'Checkbox List',
	alias: 'Umbraco.CheckboxList',
	meta: {
		defaultPropertyEditorUiAlias: 'Umb.PropertyEditorUi.CheckboxList',
	},
};
