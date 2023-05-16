import type { ManifestPropertyEditorUI } from '@umbraco-cms/backoffice/extension-registry';

export const manifest: ManifestPropertyEditorUI = {
	type: 'propertyEditorUI',
	alias: 'Umb.PropertyEditorUI.BlockGrid.StylesheetPicker',
	name: 'Block Grid Stylesheet Picker Property Editor UI',
	loader: () => import('./property-editor-ui-block-grid-stylesheet-picker.element'),
	meta: {
		label: 'Block Grid Stylesheet Picker',
		propertyEditorModel: '',
		icon: 'umb:autofill',
		group: 'blocks',
	},
};
