import type { ManifestPropertyEditorUi } from '@umbraco-cms/backoffice/extension-registry';

export const manifest: ManifestPropertyEditorUi = {
	type: 'propertyEditorUi',
	alias: 'Umb.PropertyEditorUi.BlockGrid.StylesheetPicker',
	name: 'Block Grid Stylesheet Picker Property Editor UI',
	loader: () => import('./property-editor-ui-block-grid-stylesheet-picker.element.js'),
	meta: {
		label: 'Block Grid Stylesheet Picker',
		propertyEditorModel: '',
		icon: 'umb:autofill',
		group: 'blocks',
	},
};
