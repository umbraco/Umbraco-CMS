import type { ManifestPropertyEditorUI } from '@umbraco-cms/backoffice/extension-registry';

export const manifest: ManifestPropertyEditorUI = {
	type: 'propertyEditorUI',
	alias: 'Umb.PropertyEditorUI.MemberPicker',
	name: 'Member Picker Property Editor UI',
	loader: () => import('./property-editor-ui-member-picker.element'),
	meta: {
		label: 'Member Picker',
		propertyEditorModel: 'Umbraco.MemberPicker',
		icon: 'umb:user',
		group: 'people',
	},
};
