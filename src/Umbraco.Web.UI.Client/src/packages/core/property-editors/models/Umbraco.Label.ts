import type { ManifestPropertyEditorModel } from '@umbraco-cms/backoffice/extension-registry';

export const manifest: ManifestPropertyEditorModel = {
	type: 'propertyEditorModel',
	name: 'Label',
	alias: 'Umbraco.Label',
	meta: {
		defaultUI: 'Umb.PropertyEditorUI.Label',
		settings: {
			properties: [
				{
					alias: 'umbracoDataValueType',
					label: 'Value type',
					description: 'The type of value to store',
					propertyEditorUI: 'Umb.PropertyEditorUI.ValueType',
				},
			],
		},
	},
};
