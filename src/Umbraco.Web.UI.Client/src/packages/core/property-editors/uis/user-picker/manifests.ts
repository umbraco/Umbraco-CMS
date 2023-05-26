import type { ManifestPropertyEditorUI } from '@umbraco-cms/backoffice/extension-registry';

export const manifest: ManifestPropertyEditorUI = {
	type: 'propertyEditorUI',
	alias: 'Umb.PropertyEditorUI.UserPicker',
	name: 'User Picker Property Editor UI',
	loader: () => import('./property-editor-ui-user-picker.element.js'),
	meta: {
		label: 'User Picker',
		propertyEditorModel: 'Umbraco.UserPicker',
		icon: 'umb:user',
		group: 'people',
	},
};
