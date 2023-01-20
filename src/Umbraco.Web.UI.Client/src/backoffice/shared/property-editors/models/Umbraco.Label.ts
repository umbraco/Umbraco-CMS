import type { ManifestPropertyEditorModel } from '@umbraco-cms/models';

export const manifest: ManifestPropertyEditorModel = {
	type: 'propertyEditorModel',
	name: 'Label',
	alias: 'Umbraco.Label',
	meta: {
		config: {
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
