import type { ManifestPropertyEditorSchema } from '@umbraco-cms/backoffice/property-editor';

export const manifest: ManifestPropertyEditorSchema = {
	type: 'propertyEditorSchema',
	name: 'Label',
	alias: 'Umbraco.Label',
	meta: {
		defaultPropertyEditorUiAlias: 'Umb.PropertyEditorUi.Label',
	},
};
