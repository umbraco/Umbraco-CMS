import type { ManifestPropertyEditorModel } from '@umbraco-cms/models';

export const manifest: ManifestPropertyEditorModel = {
	type: 'propertyEditorModel',
	name: 'Tags',
	alias: 'Umbraco.Tags',
	meta: {
		config: {
			properties: [
				{
					alias: 'group',
					label: 'Define a tag group',
					description: '',
					propertyEditorUI: 'Umb.PropertyEditorUI.TextBox',
				},
				{
					alias: 'storageType',
					label: 'Storage Type',
					description: '',
					propertyEditorUI: 'Umb.PropertyEditorUI.Dropdown',
				},
			],
			defaultData: [
				{
					alias: 'group',
					value: 'default',
				},
				{
					alias: 'storageType',
					value: 'Json',
				},
			],
		},
	},
};
