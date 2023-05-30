import type { ManifestPropertyEditorUI } from '@umbraco-cms/backoffice/extension-registry';

export const manifest: ManifestPropertyEditorUI = {
	type: 'propertyEditorUI',
	alias: 'Umb.PropertyEditorUI.MemberGroupPicker',
	name: 'Member Group Picker Property Editor UI',
	loader: () => import('./property-editor-ui-member-group-picker.element.js'),
	meta: {
		label: 'Member Group Picker',
		propertyEditorModel: 'Umbraco.MemberGroupPicker',
		icon: 'umb:users-alt',
		group: 'people',
	},
};
