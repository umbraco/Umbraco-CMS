import type { ManifestPropertyEditorUi } from '@umbraco-cms/backoffice/extension-registry';

export const manifest: ManifestPropertyEditorUi = {
	type: 'propertyEditorUi',
	alias: 'Umb.PropertyEditorUi.Collection.BulkActionPermissions',
	name: 'Collection View Bulk Action Permissions Property Editor UI',
	element: () => import('./permissions.element.js'),
	meta: {
		label: 'Collection View Bulk Action Permissions',
		icon: 'icon-autofill',
		group: 'lists',
	},
};
