import type { ManifestPropertyEditorSchema } from '@umbraco-cms/backoffice/property-editor';

export const manifest: ManifestPropertyEditorSchema = {
	type: 'propertyEditorSchema',
	name: 'Date Time with Time Zone',
	alias: 'Umbraco.DateTime2.DateTimeWithTimeZone',
	meta: {
		defaultPropertyEditorUiAlias: 'Umb.PropertyEditorUi.DateTimeWithTimeZonePicker',
	},
};
