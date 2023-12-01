import type { ManifestPropertyEditorUi } from '@umbraco-cms/backoffice/extension-registry';

export const manifest: ManifestPropertyEditorUi = {
	type: 'propertyEditorUi',
	alias: 'Umb.PropertyEditorUi.ColorEditor',
	name: 'Color Editor Property Editor UI',
	js: () => import('./property-editor-ui-color-editor.element.js'),
	meta: {
		label: 'Color Editor',
		icon: 'icon-page-add',
		group: 'Umbraco.DropDown.Flexible',
	},
};
