import type { ManifestPropertyEditorSchema } from '@umbraco-cms/backoffice/property-editor';

export const manifest: ManifestPropertyEditorSchema = {
	type: 'propertyEditorSchema',
	name: 'Date/Time',
	alias: 'Umbraco.TrueFalse',
	meta: {
		defaultPropertyEditorUiAlias: 'Umb.PropertyEditorUi.Toggle',
	},
};
