import type { ManifestPropertyEditorUi } from '@umbraco-cms/backoffice/extension-registry';

export const manifest: ManifestPropertyEditorUi = {
	type: 'propertyEditorUi',
	alias: 'Umb.PropertyEditorUi.IconPicker',
	name: 'Icon Picker Property Editor UI',
	loader: () => import('./property-editor-ui-icon-picker.element.js'),
	meta: {
		label: 'Icon Picker',
		propertyEditorAlias: 'Umbraco.JSON',
		icon: 'umb:autofill',
		group: 'common',
	},
};
