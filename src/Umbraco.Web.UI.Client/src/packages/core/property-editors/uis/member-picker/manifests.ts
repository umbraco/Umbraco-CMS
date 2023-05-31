import type { ManifestPropertyEditorUi } from '@umbraco-cms/backoffice/extension-registry';

export const manifest: ManifestPropertyEditorUi = {
	type: 'propertyEditorUi',
	alias: 'Umb.PropertyEditorUi.MemberPicker',
	name: 'Member Picker Property Editor UI',
	loader: () => import('./property-editor-ui-member-picker.element.js'),
	meta: {
		label: 'Member Picker',
		propertyEditorModel: 'Umbraco.MemberPicker',
		icon: 'umb:user',
		group: 'people',
	},
};
