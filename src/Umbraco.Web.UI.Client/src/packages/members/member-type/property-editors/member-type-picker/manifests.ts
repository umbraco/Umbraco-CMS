import type { ManifestPropertyEditorUi } from '@umbraco-cms/backoffice/property-editor';

export const manifest: ManifestPropertyEditorUi = {
	type: 'propertyEditorUi',
	alias: 'Umb.PropertyEditorUi.MemberTypePicker',
	name: 'Member Type Picker Property Editor UI',
	element: () => import('./property-editor-ui-member-type-picker.element.js'),
	meta: {
		label: 'Member Type Picker',
		icon: 'icon-user',
		group: '#propertyEditorUIGroups_advanced',
	},
};
