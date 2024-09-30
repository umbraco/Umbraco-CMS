import type { ManifestPropertyEditorUi } from '@umbraco-cms/backoffice/property-editor';

export const manifest: ManifestPropertyEditorUi = {
	type: 'propertyEditorUi',
	alias: 'Umb.PropertyEditorUi.ColorSwatchesEditor',
	name: 'Color Swatches Editor Property Editor UI',
	element: () => import('./property-editor-ui-color-swatches-editor.element.js'),
	meta: {
		label: 'Color Swatches Editor',
		icon: 'icon-page-add',
		group: 'pickers',
	},
};
