import { manifest as bulkActionPermissions } from './config/bulk-action-permissions/manifests.js';
import { manifest as columnConfiguration } from './config/column/manifests.js';
import { manifest as layoutConfiguration } from './config/layout/manifests.js';
import { manifest as orderBy } from './config/order-by/manifests.js';
import { manifest as schema } from './Umbraco.ListView.js';
import type { ManifestPropertyEditorUi, ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

const propertyEditorUiManifest: ManifestPropertyEditorUi = {
	type: 'propertyEditorUi',
	alias: 'Umb.PropertyEditorUi.Collection',
	name: 'Collection Property Editor UI',
	element: () => import('./property-editor-ui-collection.element.js'),
	meta: {
		label: 'Collection',
		propertyEditorSchemaAlias: 'Umbraco.ListView',
		icon: 'icon-layers',
		group: 'lists',
		settings: {
			properties: [
				{
					alias: 'layouts',
					label: 'Layouts',
					description: 'The properties that will be displayed for each column.',
					propertyEditorUiAlias: 'Umb.PropertyEditorUi.Collection.LayoutConfiguration',
				},
				{
					alias: 'orderBy',
					label: 'Order By',
					description: 'The default sort order for the list.',
					propertyEditorUiAlias: 'Umb.PropertyEditorUi.Collection.OrderBy',
				},
				{
					alias: 'orderDirection',
					label: 'Order Direction',
					propertyEditorUiAlias: 'Umb.PropertyEditorUi.OrderDirection',
				},
				{
					alias: 'pageSize',
					label: 'Page Size',
					description: 'Number of items per page.',
					propertyEditorUiAlias: 'Umb.PropertyEditorUi.Integer',
					config: [{ alias: 'min', value: 0 }],
				},
				{
					alias: 'bulkActionPermissions',
					label: 'Bulk Action Permissions',
					description: 'The bulk actions that are allowed from the list view.',
					propertyEditorUiAlias: 'Umb.PropertyEditorUi.Collection.BulkActionPermissions',
				},
				{
					alias: 'icon',
					label: 'Content app icon',
					description: 'The icon of the listview content app.',
					propertyEditorUiAlias: 'Umb.PropertyEditorUi.IconPicker',
				},
				{
					alias: 'tabName',
					label: 'Content app name',
					description: 'The name of the listview content app (default if empty: Child Items).',
					propertyEditorUiAlias: 'Umb.PropertyEditorUi.TextBox',
				},
				{
					alias: 'showContentFirst',
					label: 'Show Content App First',
					description: 'Enable this to show the content app by default instead of the list view app.',
					propertyEditorUiAlias: 'Umb.PropertyEditorUi.Toggle',
				},
			],
			defaultData: [
				{
					alias: 'includeProperties',
					value: [
						{ header: 'Sort', alias: 'sortOrder', isSystem: 1 },
						{ header: 'Last edited', alias: 'updateDate', isSystem: 1 },
						{ header: 'Created by', alias: 'creator', isSystem: 1 },
					],
				},
				{
					alias: 'layouts',
					value: [
						{ name: 'Table', icon: 'icon-list', collectionView: 'Umb.CollectionView.Document.Table' },
						{ name: 'Grid', icon: 'icon-grid', collectionView: 'Umb.CollectionView.Document.Grid' },
					],
				},
				{ alias: 'pageSize', value: 10 },
				{ alias: 'orderBy', value: 'sortOrder' },
				{ alias: 'orderDirection', value: 'desc' },
				{
					alias: 'bulkActionPermissions',
					value: { allowBulkPublish: true, allowBulkUnpublish: true, allowBulkCopy: true },
				},
				{ alias: 'icon', value: 'icon-list' },
			],
		},
	},
};

export const manifests: Array<ManifestTypes> = [
	propertyEditorUiManifest,
	bulkActionPermissions,
	columnConfiguration,
	layoutConfiguration,
	orderBy,
	schema,
];
