import type { ManifestPropertyEditorSchema } from '@umbraco-cms/backoffice/extension-registry';

export const manifest: ManifestPropertyEditorSchema = {
	type: 'propertyEditorSchema',
	name: 'Date/Time',
	alias: 'Umbraco.DateTime',
	meta: {
		defaultPropertyEditorUiAlias: 'Umb.PropertyEditorUi.DatePicker',
	},
};
