import type { ManifestPropertyEditorUi } from '@umbraco-cms/backoffice/extension-registry';

export const manifest: ManifestPropertyEditorUi = {
	type: 'propertyEditorUi',
	alias: 'Umb.PropertyEditorUi.MemberPicker',
	name: 'Member Picker Property Editor UI',
	element: () => import('./property-editor-ui-member-picker.element.js'),
	meta: {
		label: 'Member Picker',
		propertyEditorSchemaAlias: 'Umbraco.MemberPicker',
		icon: 'icon-user',
		group: 'people',
	},
};
