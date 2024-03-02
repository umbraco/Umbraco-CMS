import type { ManifestPropertyEditorUi } from '@umbraco-cms/backoffice/extension-registry';

export const manifest: ManifestPropertyEditorUi = {
	type: 'propertyEditorUi',
	alias: 'Umb.PropertyEditorUi.CollectionView.BulkActionPermissions',
	name: 'Collection View Bulk Action Permissions Property Editor UI',
	element: () => import('./property-editor-ui-collection-view-bulk-action-permissions.element.js'),
	meta: {
		label: 'Collection View Bulk Action Permissions',
		icon: 'icon-autofill',
		group: 'lists',
	},
};
