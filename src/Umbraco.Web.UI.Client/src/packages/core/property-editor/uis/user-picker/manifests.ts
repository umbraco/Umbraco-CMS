import type { ManifestPropertyEditorUi } from '@umbraco-cms/backoffice/extension-registry';

export const manifest: ManifestPropertyEditorUi = {
	type: 'propertyEditorUi',
	alias: 'Umb.PropertyEditorUi.UserPicker',
	name: 'User Picker Property Editor UI',
	element: () => import('./property-editor-ui-user-picker.element.js'),
	meta: {
		label: 'User Picker',
		propertyEditorSchemaAlias: 'Umbraco.UserPicker',
		icon: 'icon-user',
		group: 'people',
	},
};
