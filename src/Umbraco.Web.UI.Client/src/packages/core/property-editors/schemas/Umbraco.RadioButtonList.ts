import type { ManifestPropertyEditorSchema } from '@umbraco-cms/backoffice/extension-registry';

export const manifest: ManifestPropertyEditorSchema = {
	type: 'propertyEditorSchema',
	name: 'Radio Button List',
	alias: 'Umbraco.RadioButtonList',
	meta: {
		defaultPropertyEditorUiAlias: 'Umb.PropertyEditorUi.RadioButtonList',
	},
};
