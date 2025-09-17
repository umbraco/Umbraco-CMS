import type { ManifestPropertyEditorSchema } from '@umbraco-cms/backoffice/property-editor';

export const manifest: ManifestPropertyEditorSchema = {
	type: 'propertyEditorSchema',
	name: 'Date Time Unspecified',
	alias: 'Umbraco.DateTime2.DateTime',
	meta: {
		defaultPropertyEditorUiAlias: 'Umb.PropertyEditorUi.DateTimePicker',
	},
};
