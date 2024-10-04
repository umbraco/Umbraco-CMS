import type { ManifestPropertyEditorSchema } from '@umbraco-cms/backoffice/property-editor';

export const manifest: ManifestPropertyEditorSchema = {
	type: 'propertyEditorSchema',
	name: 'Tags',
	alias: 'Umbraco.Tags',
	meta: {
		defaultPropertyEditorUiAlias: 'Umb.PropertyEditorUi.Tags',
		settings: {
			properties: [
				{
					alias: 'group',
					label: 'Define a tag group',
					description: '',
					propertyEditorUiAlias: 'Umb.PropertyEditorUi.TextBox',
				},
				{
					alias: 'storageType',
					label: 'Storage Type',
					description: '',
					propertyEditorUiAlias: 'Umb.PropertyEditorUi.Select',
					config: [
						{
							alias: 'items',
							value: ['Csv', 'Json'],
						},
					],
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
