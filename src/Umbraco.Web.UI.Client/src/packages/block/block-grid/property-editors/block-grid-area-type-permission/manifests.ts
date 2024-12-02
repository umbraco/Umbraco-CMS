import type { ManifestPropertyEditorUi } from '@umbraco-cms/backoffice/property-editor';

export const manifest: ManifestPropertyEditorUi = {
	type: 'propertyEditorUi',
	alias: 'Umb.PropertyEditorUi.BlockGridAreaTypePermission',
	name: 'Block Grid Area Type Permission Configuration Property Editor UI',
	element: () => import('./block-grid-area-type-permission.element.js'),
	meta: {
		label: 'Block Grid Area Type Permissions',
		icon: 'icon-document',
		group: 'common',
	},
};
