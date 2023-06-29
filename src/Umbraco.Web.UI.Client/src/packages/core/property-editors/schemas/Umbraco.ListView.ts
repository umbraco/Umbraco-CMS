import type { ManifestPropertyEditorSchema } from '@umbraco-cms/backoffice/extension-registry';

export const manifest: ManifestPropertyEditorSchema = {
	type: 'propertyEditorSchema',
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
					propertyEditorUiAlias: 'Umb.PropertyEditorUi.Number',
				},
				{
					alias: 'orderDirection',
					label: 'Order Direction',
					propertyEditorUiAlias: 'Umb.PropertyEditorUi.OrderDirection',
				},
				{
					alias: 'includeProperties',
					label: 'Columns Displayed',
					description: 'The properties that will be displayed for each column',
					propertyEditorUiAlias: 'Umb.PropertyEditorUi.CollectionView.ColumnConfiguration',
				},
				{
					alias: 'orderBy',
					label: 'Order By',
					description: 'The properties that will be displayed for each column',
					propertyEditorUiAlias: 'Umb.PropertyEditorUi.CollectionView.OrderBy',
				},
				{
					alias: 'bulkActionPermissions',
					label: 'Bulk Action Permissions',
					description: 'The properties that will be displayed for each column',
					propertyEditorUiAlias: 'Umb.PropertyEditorUi.CollectionView.BulkActionPermissions',
				},
			],
		},
	},
};
