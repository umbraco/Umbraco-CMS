import type { ManifestPropertyEditorSchema } from '@umbraco-cms/backoffice/property-editor';

export const manifest: ManifestPropertyEditorSchema = {
	type: 'propertyEditorSchema',
	name: 'Label',
	alias: 'Umbraco.Label',
	meta: {
		defaultPropertyEditorUiAlias: 'Umb.PropertyEditorUi.Label',
		settings: {
			properties: [
				{
					alias: 'umbracoDataValueType',
					label: 'Value type',
					description: 'The type of value to store',
					propertyEditorUiAlias: 'Umb.PropertyEditorUi.ValueType',
				},
			],
		},
	},
};
