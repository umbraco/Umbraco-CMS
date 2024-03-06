import { manifest as bulkActionPermissions } from './config/bulk-action-permissions/manifests.js';
import { manifest as columnConfiguration } from './config/column-configuration/manifests.js';
import { manifest as layoutConfiguration } from './config/layout-configuration/manifests.js';
import { manifest as orderBy } from './config/order-by/manifests.js';
import type { ManifestPropertyEditorUi } from '@umbraco-cms/backoffice/extension-registry';

const manifest: ManifestPropertyEditorUi = {
	type: 'propertyEditorUi',
	alias: 'Umb.PropertyEditorUi.CollectionView',
	name: 'Collection View Property Editor UI',
	element: () => import('./property-editor-ui-collection-view.element.js'),
	meta: {
		label: 'Collection View',
		propertyEditorSchemaAlias: 'Umbraco.ListView',
		icon: 'icon-bulleted-list',
		group: 'lists',
		settings: {
			properties: [
				{
					alias: 'layouts',
					label: 'Layouts',
					description: 'The properties that will be displayed for each column.',
					propertyEditorUiAlias: 'Umb.PropertyEditorUi.CollectionView.LayoutConfiguration',
				},
				{
					alias: 'pageSize',
					label: 'Page Size',
					description: 'Number of items per page.',
					propertyEditorUiAlias: 'Umb.PropertyEditorUi.Number',
				},
				{
					alias: 'orderBy',
					label: 'Order By',
					description: 'The default sort order for the list.',
					propertyEditorUiAlias: 'Umb.PropertyEditorUi.CollectionView.OrderBy',
				},
				{
					alias: 'orderDirection',
					label: 'Order Direction',
					propertyEditorUiAlias: 'Umb.PropertyEditorUi.OrderDirection',
				},
				{
					alias: 'bulkActionPermissions',
					label: 'Bulk Action Permissions',
					description: 'The bulk actions that are allowed from the list view.',
					propertyEditorUiAlias: 'Umb.PropertyEditorUi.CollectionView.BulkActionPermissions',
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
				{
					alias: 'useInfiniteEditor',
					label: 'Edit in Infinite Editor',
					description: 'Enable this to use infinite editing to edit the content of the list view.',
					propertyEditorUiAlias: 'Umb.PropertyEditorUi.Toggle',
				},
			],
		},
	},
};

const config: Array<ManifestPropertyEditorUi> = [
	bulkActionPermissions,
	columnConfiguration,
	layoutConfiguration,
	orderBy,
];

export const manifests = [manifest, ...config];
