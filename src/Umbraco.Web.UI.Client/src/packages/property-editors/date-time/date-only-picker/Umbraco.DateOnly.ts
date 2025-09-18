import type { ManifestPropertyEditorSchema } from '@umbraco-cms/backoffice/property-editor';

export const manifest: ManifestPropertyEditorSchema = {
	type: 'propertyEditorSchema',
	name: 'Date Only',
	alias: 'Umbraco.DateOnly',
	meta: {
		defaultPropertyEditorUiAlias: 'Umb.PropertyEditorUi.DateOnlyPicker',
	},
};
