import type { ManifestPropertyEditorUi } from '@umbraco-cms/backoffice/property-editor';

/** @deprecated No longer used internally. This will be removed in Umbraco 17. [LK] */
export const manifest: ManifestPropertyEditorUi = {
	type: 'propertyEditorUi',
	alias: 'Umb.PropertyEditorUi.Collection.BulkActionPermissions',
	name: 'Collection Bulk Action Permissions Property Editor UI',
	element: () => import('./permissions.element.js'),
	meta: {
		label: 'Collection Bulk Action Permissions',
		icon: 'icon-autofill',
		group: 'lists',
	},
};
