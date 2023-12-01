import type { ManifestPropertyEditorUi } from '@umbraco-cms/backoffice/extension-registry';

export const manifest: ManifestPropertyEditorUi = {
	type: 'propertyEditorUi',
	alias: 'Umb.PropertyEditorUi.ColorPicker.Prevalues',
	name: 'Color Picker Prevalues Property Editor UI',
	js: () => import('./property-editor-ui-color-picker-prevalues.element.js'),
	meta: {
		label: 'Color Picker Prevalues',
		icon: 'icon-page-add',
		group: 'Umbraco.DropDown.Flexible',
	},
};
