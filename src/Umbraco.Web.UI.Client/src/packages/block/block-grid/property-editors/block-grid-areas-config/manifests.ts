import type { ManifestPropertyEditorUi } from '@umbraco-cms/backoffice/property-editor';

export const manifest: ManifestPropertyEditorUi = {
	type: 'propertyEditorUi',
	alias: 'Umb.PropertyEditorUi.BlockGridAreasConfig',
	name: 'Block Grid Areas Configuration Property Editor UI',
	element: () => import('./property-editor-ui-block-grid-areas-config.element.js'),
	meta: {
		label: 'Block Grid Areas Configuration',
		icon: 'icon-document',
		group: 'common',
	},
};
