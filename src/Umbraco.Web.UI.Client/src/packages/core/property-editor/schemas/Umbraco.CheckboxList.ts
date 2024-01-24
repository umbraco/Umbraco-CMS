import type { ManifestPropertyEditorSchema } from '@umbraco-cms/backoffice/extension-registry';

export const manifest: ManifestPropertyEditorSchema = {
	type: 'propertyEditorSchema',
	name: 'Checkbox List',
	alias: 'Umbraco.CheckboxList',
	meta: {
		defaultPropertyEditorUiAlias: 'Umb.PropertyEditorUi.CheckboxList',
	},
};
