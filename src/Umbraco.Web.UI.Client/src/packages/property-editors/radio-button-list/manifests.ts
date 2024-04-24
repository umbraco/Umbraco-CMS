import type { ManifestPropertyEditorUi } from '@umbraco-cms/backoffice/extension-registry';

export const manifest: ManifestPropertyEditorUi = {
	type: 'propertyEditorUi',
	alias: 'Umb.PropertyEditorUi.RadioButtonList',
	name: 'Radio Button List Property Editor UI',
	element: () => import('./property-editor-ui-radio-button-list.element.js'),
	meta: {
		label: 'Radio Button List',
		propertyEditorSchemaAlias: 'Umbraco.RadioButtonList',
		icon: 'icon-target',
		group: 'lists',
	},
};
