import type { ManifestPropertyEditorSchema } from '@umbraco-cms/backoffice/property-editor';

export const manifest: ManifestPropertyEditorSchema = {
	type: 'propertyEditorSchema',
	name: 'Date Only',
	alias: 'Umbraco.DateTime2.DateOnly',
	meta: {
		defaultPropertyEditorUiAlias: 'Umb.PropertyEditorUi.DateOnlyPicker',
	},
};
