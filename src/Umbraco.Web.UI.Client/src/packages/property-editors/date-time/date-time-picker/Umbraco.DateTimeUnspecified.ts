import type { ManifestPropertyEditorSchema } from '@umbraco-cms/backoffice/property-editor';

export const manifest: ManifestPropertyEditorSchema = {
	type: 'propertyEditorSchema',
	name: 'Date Time (unspecified)',
	alias: 'Umbraco.DateTimeUnspecified',
	meta: {
		defaultPropertyEditorUiAlias: 'Umb.PropertyEditorUi.DateTimePicker',
	},
};
