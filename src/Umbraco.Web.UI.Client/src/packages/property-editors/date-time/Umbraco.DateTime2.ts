import type { ManifestPropertyEditorSchema } from '@umbraco-cms/backoffice/property-editor';

export const manifest: ManifestPropertyEditorSchema = {
	type: 'propertyEditorSchema',
	name: 'Date Time 2',
	alias: 'Umbraco.DateTime2',
	meta: {
		defaultPropertyEditorUiAlias: 'Umb.PropertyEditorUi.DateTimePicker',
	},
};
