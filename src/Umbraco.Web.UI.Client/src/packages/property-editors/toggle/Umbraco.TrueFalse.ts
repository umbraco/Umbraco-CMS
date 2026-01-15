import type { ManifestPropertyEditorSchema } from '@umbraco-cms/backoffice/property-editor';

export const manifest: ManifestPropertyEditorSchema = {
	type: 'propertyEditorSchema',
	name: 'True/False',
	alias: 'Umbraco.TrueFalse',
	meta: {
		defaultPropertyEditorUiAlias: 'Umb.PropertyEditorUi.Toggle',
	},
};
