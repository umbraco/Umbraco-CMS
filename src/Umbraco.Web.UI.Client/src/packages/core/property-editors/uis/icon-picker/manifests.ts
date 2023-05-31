import type { ManifestPropertyEditorUI } from '@umbraco-cms/backoffice/extension-registry';

export const manifest: ManifestPropertyEditorUI = {
	type: 'propertyEditorUI',
	alias: 'Umb.PropertyEditorUi.IconPicker',
	name: 'Icon Picker Property Editor UI',
	loader: () => import('./property-editor-ui-icon-picker.element.js'),
	meta: {
		label: 'Icon Picker',
		propertyEditorModel: 'Umbraco.JSON',
		icon: 'umb:autofill',
		group: 'common',
	},
};
