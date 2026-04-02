import type { ManifestPropertyEditorSchema } from '@umbraco-cms/backoffice/property-editor';

export const manifest: ManifestPropertyEditorSchema = {
	type: 'propertyEditorSchema',
	name: 'Date Time (with time zone)',
	alias: 'Umbraco.DateTimeWithTimeZone',
	meta: {
		defaultPropertyEditorUiAlias: 'Umb.PropertyEditorUi.DateTimeWithTimeZonePicker',
	},
};
