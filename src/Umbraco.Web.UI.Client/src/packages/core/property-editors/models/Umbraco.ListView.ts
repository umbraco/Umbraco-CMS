import type { ManifestPropertyEditorModel } from '@umbraco-cms/backoffice/extension-registry';

export const manifest: ManifestPropertyEditorModel = {
	type: 'propertyEditorModel',
	name: 'List View',
	alias: 'Umbraco.ListView',
	meta: {
		defaultPropertyEditorUiAlias: 'Umb.PropertyEditorUi.ListView',
		settings: {
			properties: [
				{
					alias: 'pageSize',
					label: 'Page Size',
					description: 'Number of items per page.',
					propertyEditorUi: 'Umb.PropertyEditorUi.Number',
				},
				{
					alias: 'orderDirection',
					label: 'Order Direction',
					propertyEditorUi: 'Umb.PropertyEditorUi.OrderDirection',
				},
				{
					alias: 'includeProperties',
					label: 'Columns Displayed',
					description: 'The properties that will be displayed for each column',
					propertyEditorUi: 'Umb.PropertyEditorUi.CollectionView.ColumnConfiguration',
				},
				{
					alias: 'orderBy',
					label: 'Order By',
					description: 'The properties that will be displayed for each column',
					propertyEditorUi: 'Umb.PropertyEditorUi.CollectionView.OrderBy',
				},
				{
					alias: 'bulkActionPermissions',
					label: 'Bulk Action Permissions',
					description: 'The properties that will be displayed for each column',
					propertyEditorUi: 'Umb.PropertyEditorUi.CollectionView.BulkActionPermissions',
				},
			],
		},
	},
};
