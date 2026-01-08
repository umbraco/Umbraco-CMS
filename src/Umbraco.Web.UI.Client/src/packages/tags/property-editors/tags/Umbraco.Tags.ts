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
					label: 'Tag group',
					description: '',
					propertyEditorUiAlias: 'Umb.PropertyEditorUi.TextBox',
				},
				{
					alias: 'storageType',
					label: 'Storage Type',
					description:
						'Select whether to store the tags in cache as JSON (default) or CSV format. Notice that CSV does not support commas in the tag value.',
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
