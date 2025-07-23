import type { ManifestPropertyEditorUi } from '@umbraco-cms/backoffice/property-editor';

export const manifest: ManifestPropertyEditorUi = {
	type: 'propertyEditorUi',
	alias: 'Umb.PropertyEditorUi.StylesheetPicker',
	name: 'Stylesheet Picker Property Editor UI',
	js: () => import('./property-editor-ui-stylesheet-picker.element.js'),
	meta: {
		label: 'Stylesheet Picker',
		icon: 'icon-document',
		group: 'common',
	},
};
