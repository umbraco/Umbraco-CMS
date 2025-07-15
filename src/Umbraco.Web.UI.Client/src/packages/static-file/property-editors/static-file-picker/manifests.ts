import type { ManifestPropertyEditorUi } from '@umbraco-cms/backoffice/property-editor';

export const manifest: ManifestPropertyEditorUi = {
	type: 'propertyEditorUi',
	alias: 'Umb.PropertyEditorUi.StaticFilePicker',
	name: 'Static File Picker Property Editor UI',
	js: () => import('./property-editor-ui-static-file-picker.element.js'),
	meta: {
		label: 'Static File Picker',
		icon: 'icon-document',
		group: 'common',
	},
};
