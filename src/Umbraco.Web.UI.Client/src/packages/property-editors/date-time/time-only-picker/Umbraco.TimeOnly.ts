import type { ManifestPropertyEditorSchema } from '@umbraco-cms/backoffice/property-editor';

export const manifest: ManifestPropertyEditorSchema = {
	type: 'propertyEditorSchema',
	name: 'Time Only',
	alias: 'Umbraco.TimeOnly',
	meta: {
		defaultPropertyEditorUiAlias: 'Umb.PropertyEditorUi.TimeOnlyPicker',
	},
};
