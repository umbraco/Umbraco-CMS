import type { ManifestPropertyEditorModel } from '@umbraco-cms/backoffice/extension-registry';

export const manifest: ManifestPropertyEditorModel = {
	type: 'propertyEditorModel',
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
