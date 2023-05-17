import type { ManifestPropertyEditorModel } from '@umbraco-cms/backoffice/extension-registry';

export const manifest: ManifestPropertyEditorModel = {
	type: 'propertyEditorModel',
	name: 'List View',
	alias: 'Umbraco.ListView',
	meta: {
		config: {
			properties: [
				{
					alias: 'pageSize',
					label: 'Page Size',
					description: 'Number of items per page.',
					propertyEditorUI: 'Umb.PropertyEditorUI.Number',
				},
				{
					alias: 'orderDirection',
					label: 'Order Direction',
					propertyEditorUI: 'Umb.PropertyEditorUI.OrderDirection',
				},
				{
					alias: 'includeProperties',
					label: 'Columns Displayed',
					description: 'The properties that will be displayed for each column',
					propertyEditorUI: 'Umb.PropertyEditorUI.CollectionView.ColumnConfiguration',
				},
				{
					alias: 'orderBy',
					label: 'Order By',
					description: 'The properties that will be displayed for each column',
					propertyEditorUI: 'Umb.PropertyEditorUI.CollectionView.OrderBy',
				},
				{
					alias: 'bulkActionPermissions',
					label: 'Bulk Action Permissions',
					description: 'The properties that will be displayed for each column',
					propertyEditorUI: 'Umb.PropertyEditorUI.CollectionView.BulkActionPermissions',
				},
			],
		},
	},
};
