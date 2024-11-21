import type { ManifestPropertyEditorSchema } from '@umbraco-cms/backoffice/property-editor';

export const manifest: ManifestPropertyEditorSchema = {
	type: 'propertyEditorSchema',
	name: 'List View',
	alias: 'Umbraco.ListView',
	meta: {
		defaultPropertyEditorUiAlias: 'Umb.PropertyEditorUi.Collection',
		settings: {
			properties: [
				{
					alias: 'includeProperties',
					label: 'Columns Displayed',
					description: 'The properties that will be displayed for each column.',
					propertyEditorUiAlias: 'Umb.PropertyEditorUi.Collection.ColumnConfiguration',
				},
			],
		},
	},
};
