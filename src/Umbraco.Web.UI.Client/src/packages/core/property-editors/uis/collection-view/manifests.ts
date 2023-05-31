import { manifest as bulkActionPermissions } from './config/bulk-action-permissions/manifests.js';
import { manifest as columnConfiguration } from './config/column-configuration/manifests.js';
import { manifest as layoutConfiguration } from './config/layout-configuration/manifests.js';
import { manifest as orderBy } from './config/order-by/manifests.js';
import type { ManifestPropertyEditorUI } from '@umbraco-cms/backoffice/extension-registry';

const manifest: ManifestPropertyEditorUI = {
	type: 'propertyEditorUI',
	alias: 'Umb.PropertyEditorUI.CollectionView',
	name: 'Collection View Property Editor UI',
	loader: () => import('./property-editor-ui-collection-view.element.js'),
	meta: {
		label: 'Collection View',
		propertyEditorModel: 'Umbraco.ListView',
		icon: 'umb:bulleted-list',
		group: 'lists',
		settings: {
			properties: [
				{
					alias: 'layouts',
					label: 'Layouts',
					description: 'The properties that will be displayed for each column',
					propertyEditorUI: 'Umb.PropertyEditorUI.CollectionView.LayoutConfiguration',
				},
				{
					alias: 'icon',
					label: 'Content app icon',
					description: 'The icon of the listview content app',
					propertyEditorUI: 'Umb.PropertyEditorUI.IconPicker',
				},
				{
					alias: 'tabName',
					label: 'Content app name',
					description: 'The name of the listview content app (default if empty: Child Items)',
					propertyEditorUI: 'Umb.PropertyEditorUI.TextBox',
				},
				{
					alias: 'showContentFirst',
					label: 'Show Content App First',
					description: 'Enable this to show the content app by default instead of the list view app',
					propertyEditorUI: 'Umb.PropertyEditorUI.Toggle',
				},
				{
					alias: 'useInfiniteEditor',
					label: 'Edit in Infinite Editor',
					description: 'Enable this to use infinite editing to edit the content of the list view',
					propertyEditorUI: 'Umb.PropertyEditorUI.Toggle',
				},
			],
		},
	},
};

const config: Array<ManifestPropertyEditorUI> = [
	bulkActionPermissions,
	columnConfiguration,
	layoutConfiguration,
	orderBy,
];

export const manifests = [manifest, ...config];
