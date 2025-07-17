import type { ManifestPropertyEditorSchema } from '@umbraco-cms/backoffice/property-editor';

export const manifest: ManifestPropertyEditorSchema = {
	type: 'propertyEditorSchema',
	name: 'Date/Time with Time Zone',
	alias: 'Umbraco.DateTimeWithTimeZone',
	meta: {
		defaultPropertyEditorUiAlias: 'Umb.PropertyEditorUi.DateWithTimeZonePicker',
	},
};
