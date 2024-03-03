import type { ManifestPropertyEditorUi } from '@umbraco-cms/backoffice/extension-registry';

export const manifest: ManifestPropertyEditorUi = {
	type: 'propertyEditorUi',
	alias: 'Umb.PropertyEditorUi.Dropdown',
	name: 'Dropdown Property Editor UI',
	element: () => import('./property-editor-ui-dropdown.element.js'),
	meta: {
		label: 'Dropdown',
		propertyEditorSchemaAlias: 'Umbraco.DropDown.Flexible',
		icon: 'icon-time',
		group: 'pickers',
	},
};
