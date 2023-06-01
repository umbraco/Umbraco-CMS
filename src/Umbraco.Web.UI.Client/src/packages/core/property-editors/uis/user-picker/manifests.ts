import type { ManifestPropertyEditorUi } from '@umbraco-cms/backoffice/extension-registry';

export const manifest: ManifestPropertyEditorUi = {
	type: 'propertyEditorUi',
	alias: 'Umb.PropertyEditorUi.UserPicker',
	name: 'User Picker Property Editor UI',
	loader: () => import('./property-editor-ui-user-picker.element.js'),
	meta: {
		label: 'User Picker',
		propertyEditorAlias: 'Umbraco.UserPicker',
		icon: 'umb:user',
		group: 'people',
	},
};
